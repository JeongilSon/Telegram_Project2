// API 기본 URL
const API_BASE_URL = location.href.includes('localhost') ? 'http://localhost:44367' : 'http://3.107.57.131';

// 채팅 관련 전역 변수 (수정됨)
let currentChatId = null;
let currentSelectedChatId = null; // 현재 선택된 사용자 추적용
let pollingInterval = null;
let fastPollingInterval = null; // 빠른 폴링용
let lastChatListUpdate = 0; // 마지막 채팅 목록 갱신 시간
let lastKnownMessageCount = 0; // 마지막 알려진 메시지 수
let lastKnownMessageContent = ''; // 마지막 알려진 메시지 내용

// 페이지 로드 시 실행
document.addEventListener('DOMContentLoaded', () => {
    // 로그인 관련 처리
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    // 현재 페이지가 index.html인 경우 인증 확인
    if (window.location.pathname.endsWith('index.html') || window.location.pathname.endsWith('/')) {
        checkAuthentication();
    }

    // 로그아웃 버튼
    const logoutButton = document.getElementById('logoutButton');
    if (logoutButton) {
        logoutButton.addEventListener('click', handleLogout);
    }

    // 현재 로그인한 사용자 표시
    const currentUser = document.getElementById('currentUser');
    if (currentUser) {
        currentUser.textContent = localStorage.getItem('username') || '관리자';
    }

    // 인덱스 페이지 초기화
    if (document.getElementById('userListTab')) {
        initializeTabs();
        loadUserList();
    }

    // 각 탭 폼 제출 이벤트 리스너 등록
    initializeFormSubmitEvents();
});

// 인증 확인 함수
function checkAuthentication() {
    if (localStorage.getItem('isAuthenticated') !== 'true') {
        window.location.href = 'login.html';
        return false;
    }
    return true;
}

// 로그인 처리 함수
async function handleLogin(event) {
    event.preventDefault();
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const loginMessage = document.getElementById('loginMessage');
    loginMessage.textContent = '';

    try {
        const response = await fetch(`${API_BASE_URL}/api/Account/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Id: username, pw: password })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                localStorage.setItem('isAuthenticated', 'true');
                localStorage.setItem('username', username);
                window.location.href = 'index.html';
            } else {
                loginMessage.textContent = result.message || '로그인에 실패했습니다.';
                loginMessage.style.color = 'red';
            }
        } else {
            const errorResult = await response.json();
            loginMessage.textContent = errorResult.message || `로그인 실패: ${response.statusText}`;
            loginMessage.style.color = 'red';
        }
    } catch (error) {
        console.error('로그인 오류:', error);
        loginMessage.textContent = '로그인 중 오류가 발생했습니다. 네트워크 연결을 확인하세요.';
        loginMessage.style.color = 'red';
    }
}

// 로그아웃 처리 함수
function handleLogout() {
    localStorage.removeItem('isAuthenticated');
    localStorage.removeItem('username');
    window.location.href = 'login.html';
}

// 탭 초기화 함수
function initializeTabs() {
    // 탭 전환 함수를 전역 범위에 정의
    window.openTab = function (tabName) {
        const tabContents = document.querySelectorAll('.tab-content');
        tabContents.forEach(tab => tab.classList.remove('active'));

        const tabButtons = document.querySelectorAll('.tab-button');
        tabButtons.forEach(button => button.classList.remove('active'));

        document.getElementById(tabName).classList.add('active');

        // 해당 탭 버튼 활성화
        const activeButton = Array.from(tabButtons).find(btn =>
            btn.getAttribute('onclick').includes(tabName));
        if (activeButton) {
            activeButton.classList.add('active');
        }

        // 탭에 따라 데이터 로드
        if (tabName === 'userListTab') loadUserList();
        if (tabName === 'chatTab') {
            loadChatList();
            
            // 채팅 사용자 선택 전에는 삭제 버튼 숨기기
            const deleteChatBtn = document.getElementById('deleteChatBtn');
            if (deleteChatBtn) {
                deleteChatBtn.style.display = 'none';
            }
        }
        if (tabName === 'managementTab') {
            loadLinks();
            loadChannels();
            loadMissions();
            loadBotMessages();
        }
        if (tabName === 'settingsTab') {
            initializeSettingsTab();
        }
    };
}

// 폼 제출 이벤트 초기화 함수
function initializeFormSubmitEvents() {
    const linkForm = document.getElementById('linkForm');
    if (linkForm) {
        linkForm.addEventListener('submit', handleLinkFormSubmit);
    }

    const channelForm = document.getElementById('channelForm');
    if (channelForm) {
        channelForm.addEventListener('submit', handleChannelFormSubmit);
    }

    const missionForm = document.getElementById('missionForm');
    if (missionForm) {
        missionForm.addEventListener('submit', handleMissionFormSubmit);
    }

    // 메시지 전송 버튼
    const sendMessageBtn = document.getElementById('sendMessageBtn');
    if (sendMessageBtn) {
        sendMessageBtn.addEventListener('click', sendMessage);
    }
    
    // 메시지 입력창에서 Enter 키 처리
    const messageInput = document.getElementById('messageInput');
    if (messageInput) {
        messageInput.addEventListener('keydown', (e) => {
            // Enter 키 눌렀을 때 메시지 전송 (Shift+Enter는 줄바꿈)
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault(); // 기본 동작 방지
                sendMessage();
            }
        });
    }

    // 검색 기능 초기화
    const userSearchInput = document.getElementById('userSearchInput');
    if (userSearchInput) {
        userSearchInput.addEventListener('input', () => {
            filterUsers(userSearchInput.value);
        });
    }

    const chatUserSearch = document.getElementById('chatUserSearch');
    if (chatUserSearch) {
        chatUserSearch.addEventListener('input', () => {
            filterChatUsers(chatUserSearch.value);
        });
    }

    const botMessageForm = document.getElementById('botMessageForm');
    if (botMessageForm) {
        botMessageForm.addEventListener('submit', handleBotMessageFormSubmit);
    }

    const chatBotTokenForm = document.getElementById('chatBotTokenForm');
    if (chatBotTokenForm) {
        chatBotTokenForm.addEventListener('submit', handleChatBotTokenFormSubmit);
    }

    const mainBotTokenForm = document.getElementById('mainBotTokenForm');
    if (mainBotTokenForm) {
        mainBotTokenForm.addEventListener('submit', handleMainBotTokenFormSubmit);
    }

    const adminAccountForm = document.getElementById('adminAccountForm');
    if (adminAccountForm) {
        adminAccountForm.addEventListener('submit', handleAdminAccountFormSubmit);
    }

    // 설정 탭 초기화
    if (document.getElementById('settingsTab')) {
        initializeSettingsTab();
    }
}

// 사용자 목록 로드 함수
async function loadUserList() {
    if (!checkAuthentication()) return;

    const userTableBody = document.getElementById('userTableBody');
    if (!userTableBody) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/User`);
        if (!response.ok) {
            throw new Error('사용자 데이터를 가져오는데 실패했습니다.');
        }

        const users = await response.json();
        displayUsers(users);
        setupPagination(users);
    } catch (error) {
        console.error('사용자 데이터 로드 오류:', error);
        userTableBody.innerHTML = `<tr><td colspan="9">사용자 데이터를 불러오는데 문제가 발생했습니다: ${error.message}</td></tr>`;
    }
}

// 사용자 데이터 표시 함수
function displayUsers(users) {
    const userTableBody = document.getElementById('userTableBody');
    if (!userTableBody) return;

    userTableBody.innerHTML = '';
    
    if (users.length === 0) {
        userTableBody.innerHTML = '<tr><td colspan="9">등록된 사용자가 없습니다.</td></tr>';
        return;
    }

    users.forEach((user, index) => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${index + 1}</td>
            <td>${user.chat_ID || ''}</td>
            <td>${user.telegram_ID || ''}</td>
            <td>${user.nickName || ''}</td>
            <td>${user.link_Move === 1 ? '✅' : '❌'}</td>
            <td>${user.channel_Move === 1 ? '✅' : '❌'}</td>
            <td>${user.mission_Complete === 1 ? '✅' : '❌'}</td>
            <td>
                <button class="action-btn action-btn-message" onclick="sendMessageToUser('${user.chat_ID}')">메시지</button>
            </td>
        <td>
            <div class="custom-response">
                <input type="text" class="custom-response-input" placeholder="맞춤 응답 입력" data-chat-id="${user.chat_ID}" value="${user.user_Question || ''}">
                <button class="action-btn action-btn-edit" onclick="sendCustomResponse('${user.chat_ID}')">저장</button>
            </div>
        </td>
        `;
        userTableBody.appendChild(row);
    });
}

// 페이지네이션 설정 함수
function setupPagination(users) {
    const pagination = document.getElementById('userPagination');
    if (!pagination) return;

    const itemsPerPage = parseInt(document.getElementById('userDisplayCount').value) || 50;
    const pageCount = Math.ceil(users.length / itemsPerPage);

    pagination.innerHTML = '';

    if (pageCount <= 1) return;

    // 이전 버튼
    const prevButton = document.createElement('button');
    prevButton.innerHTML = '&laquo;';
    prevButton.addEventListener('click', () => {
        const activePage = pagination.querySelector('.active');
        if (activePage && activePage.previousElementSibling &&
            activePage.previousElementSibling.tagName === 'BUTTON') {
            activePage.previousElementSibling.click();
        }
    });
    pagination.appendChild(prevButton);

    // 페이지 버튼들
    for (let i = 1; i <= pageCount; i++) {
        const pageButton = document.createElement('button');
        pageButton.textContent = i;
        if (i === 1) pageButton.classList.add('active');

        pageButton.addEventListener('click', () => {
            pagination.querySelectorAll('button').forEach(btn => btn.classList.remove('active'));
            pageButton.classList.add('active');

            const start = (i - 1) * itemsPerPage;
            const end = start + itemsPerPage;
            const paginatedUsers = users.slice(start, end);

            displayUsers(paginatedUsers);
        });

        pagination.appendChild(pageButton);
    }

    // 다음 버튼
    const nextButton = document.createElement('button');
    nextButton.innerHTML = '&raquo;';
    nextButton.addEventListener('click', () => {
        const activePage = pagination.querySelector('.active');
        if (activePage && activePage.nextElementSibling &&
            activePage.nextElementSibling.tagName === 'BUTTON') {
            activePage.nextElementSibling.click();
        }
    });
    pagination.appendChild(nextButton);

    // 첫 페이지 표시
    pagination.querySelector('button[textContent="1"]')?.click();
}

// 사용자 필터링 함수
function filterUsers(searchText) {
    const rows = document.querySelectorAll('#userTableBody tr');
    searchText = searchText.toLowerCase();

    rows.forEach(row => {
        let found = false;
        row.querySelectorAll('td').forEach(cell => {
            if (cell.textContent.toLowerCase().includes(searchText)) {
                found = true;
            }
        });

        row.style.display = found ? '' : 'none';
    });
}

// 사용자 상세정보 보기 함수
async function viewUserDetails(chatId) {
    if (!checkAuthentication()) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/User/${chatId}`);
        if (!response.ok) {
            throw new Error('사용자 상세정보를 가져오는데 실패했습니다.');
        }

        const user = await response.json();

        // 간단한 모달 표시 (실제 구현에서는 더 복잡한 모달 UI가 필요할 수 있음)
        alert(`
            채팅 ID: ${user.chat_ID || ''}
            텔레그램 ID: ${user.telegram_ID || ''}
            닉네임: ${user.nickName || ''}
            질문: ${user.user_Question || ''}
            링크 이동: ${user.link_Move === 1 ? '완료' : '미완료'}
            채널 가입: ${user.channel_Move === 1 ? '완료' : '미완료'}
            미션 완료: ${user.mission_Complete === 1 ? '완료' : '미완료'}
        `);
    } catch (error) {
        console.error('사용자 상세정보 로드 오류:', error);
        alert(`사용자 상세정보를 불러오는데 문제가 발생했습니다: ${error.message}`);
    }
}

// 채팅 사용자 필터링 함수
function filterChatUsers(searchText) {
    const userItems = document.querySelectorAll('#chatUserList li');
    searchText = searchText.toLowerCase();
    
    userItems.forEach(item => {
        const text = item.textContent.toLowerCase();
        item.style.display = text.includes(searchText) ? '' : 'none';
    });
}

// 채팅 메시지 로드 함수 (수정됨)
async function loadChatMessages(chatId, preserveSelection = false) {
    if (!checkAuthentication()) return;

    const messageContainer = document.getElementById('messageContainer');
    const selectedUserInfo = document.getElementById('selectedUserInfo');

    if (!messageContainer || !selectedUserInfo) return;
    
    // 이전 폴링 중지
    if (pollingInterval) {
        clearInterval(pollingInterval);
    }
    
    // 현재 채팅 ID 저장 (두 변수 모두 업데이트)
    currentChatId = chatId;
    currentSelectedChatId = chatId;

    try {
        // 사용자 정보 가져오기
        const userResponse = await fetch(`${API_BASE_URL}/api/User/${chatId}`);
        if (!userResponse.ok) {
            throw new Error('사용자 정보를 가져오는데 실패했습니다.');
        }

        const user = await userResponse.json();
        const displayName = user.nickName || user.telegram_ID || chatId;
        selectedUserInfo.innerHTML = `<strong>${displayName}</strong>`;
        
        // 채팅 메시지 가져오기
        const response = await fetch(`${API_BASE_URL}/api/Livechat/${chatId}`);
        if (!response.ok) {
            throw new Error('메시지를 가져오는데 실패했습니다.');
        }
        
        const messages = await response.json();
        
        // 메시지 컨테이너 초기화 및 표시
        messageContainer.innerHTML = '';
        messages.forEach(msg => {
            addMessageToUI(
                msg.content, 
                msg.username || (msg.isFromAdmin ? '관리자' : '사용자'), 
                msg.isFromAdmin, 
                new Date(msg.timestamp)
            );
        });
        
        // 스크롤을 아래로 이동
        messageContainer.scrollTop = messageContainer.scrollHeight;
        
        // 메시지 읽음 처리
        await fetch(`${API_BASE_URL}/api/Livechat/${chatId}/MarkAsRead`, {
            method: 'PUT'
        });
        
        // 스마트 갱신을 위한 상태 초기화
        lastKnownMessageCount = messages.length;
        lastKnownMessageContent = messages.length > 0 ? messages[messages.length - 1].content : '';
        
        // 빠른 폴링 시작 (기존 startMessagePolling 대신)
        startFastPolling();

        if (!preserveSelection) {
            // 사용자 선택 상태 업데이트
            document.querySelectorAll('#chatUserList li').forEach(li => {
                li.classList.remove('active');
            });
            
            const userItem = document.querySelector(`#chatUserList li[data-chat-id="${chatId}"]`);
            if (userItem) {
                userItem.classList.add('active');
            }
        }

        // 삭제 버튼 표시
        const deleteChatBtn = document.getElementById('deleteChatBtn');
        if (deleteChatBtn) {
            deleteChatBtn.style.display = 'block';
        }
    } catch (error) {
        console.error('채팅 메시지 로드 오류:', error);
        messageContainer.innerHTML = `<div class="error-message">메시지를 불러오는데 문제가 발생했습니다: ${error.message}</div>`;
    }
}

// 스마트 메시지 갱신 함수
function updateMessagesIfNeeded(messages) {
    const currentUICount = document.querySelectorAll('#messageContainer .message').length;
    const latestMessage = messages.length > 0 ? messages[messages.length - 1] : null;
    
    // 새 메시지가 있거나 내용이 변경된 경우
    if (messages.length !== lastKnownMessageCount || 
        (latestMessage && latestMessage.content !== lastKnownMessageContent)) {
        
        // 스크롤 위치 저장
        const messageContainer = document.getElementById('messageContainer');
        const isScrolledToBottom = messageContainer.scrollHeight - messageContainer.clientHeight <= messageContainer.scrollTop + 50;
        
        // UI 즉시 업데이트
        messageContainer.innerHTML = '';
        messages.forEach(msg => {
            addMessageToUI(
                msg.content, 
                msg.username || (msg.isFromAdmin ? '관리자' : '사용자'), 
                msg.isFromAdmin, 
                new Date(msg.timestamp)
            );
        });
        
        // 스크롤 복원
        if (isScrolledToBottom) {
            messageContainer.scrollTop = messageContainer.scrollHeight;
        }
        
        // 채팅 목록 즉시 갱신
        loadChatList();
        
        // 상태 업데이트
        lastKnownMessageCount = messages.length;
        lastKnownMessageContent = latestMessage ? latestMessage.content : '';
        
        return true; // 갱신됨
    }
    
    return false; // 갱신 안됨
}

// 초고속 폴링 시스템 (0.5초 간격)
function startFastPolling() {
    // 기존 폴링들 모두 중지
    if (pollingInterval) {
        clearInterval(pollingInterval);
    }
    if (fastPollingInterval) {
        clearInterval(fastPollingInterval);
    }
    
    // 선택된 사용자가 없으면 폴링 시작하지 않음
    if (!currentSelectedChatId) return;
    
    fastPollingInterval = setInterval(async () => {
        // 현재 선택된 사용자가 없거나 변경되었으면 폴링 중지
        if (!currentSelectedChatId || currentChatId !== currentSelectedChatId) {
            clearInterval(fastPollingInterval);
            return;
        }
        
        try {
            const response = await fetch(`${API_BASE_URL}/api/Livechat/${currentSelectedChatId}`);
            if (!response.ok) return;
            
            const messages = await response.json();
            
            // 스마트 갱신 실행
            updateMessagesIfNeeded(messages);
            
        } catch (error) {
            console.error('빠른 폴링 오류:', error);
        }
    }, 500); // 0.5초마다 확인 (기존 2초에서 대폭 단축)
}

// 현재 대화 삭제
async function deleteCurrentChat() {
    if (!currentSelectedChatId || !confirm('정말로 이 대화를 삭제하시겠습니까?\n(이 작업은 되돌릴 수 없습니다)')) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/Livechat/${currentSelectedChatId}`, {
            method: 'DELETE'
        });
        
        if (!response.ok) {
            throw new Error('대화 삭제에 실패했습니다.');
        }
        
        const result = await response.json();
        alert(result.message || '대화가 삭제되었습니다.');
        
        // 메시지 컨테이너 비우기
        const messageContainer = document.getElementById('messageContainer');
        if (messageContainer) {
            messageContainer.innerHTML = '<div class="system-message">대화가 삭제되었습니다.</div>';
        }
        
        // 선택 상태 초기화
        currentChatId = null;
        currentSelectedChatId = null;
        
        // 폴링 중지
        if (pollingInterval) {
            clearInterval(pollingInterval);
        }
        if (fastPollingInterval) {
            clearInterval(fastPollingInterval);
        }
        
        // 채팅 목록 새로고침
        loadChatList();
    } catch (error) {
        console.error('대화 삭제 오류:', error);
        alert(`대화 삭제 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 메시지 UI 추가 함수 (수정됨)
function addMessageToUI(content, sender, isFromAdmin, timestamp) {
    const messageContainer = document.getElementById('messageContainer');
    if (!messageContainer) return;
    
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${isFromAdmin ? 'sent' : 'received'}`;
    
    const timeString = `${timestamp.getHours()}:${timestamp.getMinutes().toString().padStart(2, '0')}`;
    
    messageDiv.innerHTML = `
        <div class="sender">${sender || (isFromAdmin ? '관리자' : '사용자')}</div>
        <div class="content">${content}</div>
        <div class="time">${timeString}</div>
    `;
    
    messageContainer.appendChild(messageDiv);
}

// 메시지 전송 함수 (수정됨)
async function sendMessage() {
    if (!checkAuthentication()) return;

    const messageInput = document.getElementById('messageInput');
    const selectedUser = document.querySelector('#chatUserList li.active');

    if (!messageInput || !selectedUser || !currentSelectedChatId) {
        alert('메시지를 보낼 사용자를 선택해주세요.');
        return;
    }

    const message = messageInput.value.trim();
    if (!message) return;

    try {
        // API 호출
        const response = await fetch(`${API_BASE_URL}/api/Livechat/AdminReply`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                chatId: currentSelectedChatId,
                text: message
            })
        });
        
        if (!response.ok) {
            throw new Error('메시지 전송에 실패했습니다.');
        }
        
        // 입력 필드 초기화
        messageInput.value = '';
        
        // 즉시 UI에 메시지 추가 (폴링 대기하지 않고)
        addMessageToUI(message, '관리자', true, new Date());
        
        // 스크롤을 아래로
        const messageContainer = document.getElementById('messageContainer');
        messageContainer.scrollTop = messageContainer.scrollHeight;
        
        // 메시지 전송 후 즉시 채팅 목록 갱신 (지연 제거)
        loadChatList();
        
        // 상태 업데이트 (스마트 갱신용)
        lastKnownMessageCount++;
        lastKnownMessageContent = message;
    } catch (error) {
        console.error('메시지 전송 오류:', error);
        alert(`메시지 전송 중 문제가 발생했습니다: ${error.message}`);
        // 실패시 입력 내용 복원
        messageInput.value = message;
    }
}

// 사용자에게 메시지 보내기 함수 (유저 목록에서)
function sendMessageToUser(chatId) {
    if (!checkAuthentication()) return;

    // 채팅 탭으로 이동
    openTab('chatTab');

    // 해당 사용자 선택
    setTimeout(() => {
        const userItem = document.querySelector(`#chatUserList li[data-chat-id="${chatId}"]`);
        if (userItem) {
            userItem.click();
        }
    }, 500); // 탭 로드 대기
}

// 링크 목록 로드 함수
async function loadLinks() {
    if (!checkAuthentication()) return;

    const linkTableBody = document.getElementById('linkTableBody');
    if (!linkTableBody) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Link`);
        if (!response.ok) {
            throw new Error('링크 데이터를 가져오는데 실패했습니다.');
        }

        const links = await response.json();
        
        linkTableBody.innerHTML = '';

        if (links.length === 0) {
            linkTableBody.innerHTML = '<tr><td colspan="3">등록된 링크가 없습니다.</td></tr>';
            return;
        }

        links.forEach((link, index) => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${link.link_Name || link.Link_Name || ''}</td>
                <td><a href="${link.link_Url || link.Link_Url || '#'}" target="_blank">${link.link_Url || link.Link_Url || ''}</a></td>
                <td>
                    <button class="action-btn action-btn-delete" onclick="deleteLink(${index})">삭제</button>
                </td>
            `;
            linkTableBody.appendChild(row);
        });
    } catch (error) {
        console.error('링크 목록 로드 오류:', error);
        linkTableBody.innerHTML = `<tr><td colspan="3">링크 데이터를 불러오는데 문제가 발생했습니다: ${error.message}</td></tr>`;
    }
}

// 링크 등록 처리 함수
async function handleLinkFormSubmit(event) {
    event.preventDefault();
    if (!checkAuthentication()) return;

    const linkUrl = document.getElementById('linkUrl').value;
    const linkName = document.getElementById('linkName').value;
    const linkChatContent = document.getElementById('linkChatContent').value;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Link/Input`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Link_Url: linkUrl,
                Link_Name: linkName,
                Link_Chat_Content: linkChatContent
            })
        });
        const result = await response.json();

        if (!response.ok) {
            throw new Error(result.message || '링크 저장에 실패했습니다.');
        }

        alert('링크가 성공적으로 저장되었습니다!');

        // 폼 초기화
        document.getElementById('linkForm').reset();

        // 링크 목록 새로고침
        loadLinks();
    } catch (error) {
        console.error('링크 저장 오류:', error);
        alert(`링크 저장 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 링크 삭제 함수
async function deleteLink(index) {
    if (!checkAuthentication()) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Link`);
        if (!response.ok) {
            throw new Error('링크 데이터를 가져오는데 실패했습니다.');
        }

        const links = await response.json();

        if (index < 0 || index >= links.length) {
            alert('유효하지 않은 링크 인덱스입니다.');
            return;
        }

        const link = links[index];
        const linkName = link.link_name || link.link_Name || link.linkName || link.name || '';

        if (!linkName) {
            alert('링크 이름을 찾을 수 없습니다.');
            return;
        }

        if (!confirm(`정말로 "${linkName}" 링크를 삭제하시겠습니까?`)) {
            return;
        }

        const deleteResponse = await fetch(`${API_BASE_URL}/api/Link/${encodeURIComponent(linkName)}`, {
            method: 'DELETE'
        });

        let result;
        const contentType = deleteResponse.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            result = await deleteResponse.json();
        } else {
            const text = await deleteResponse.text();
            result = { message: text || '응답이 없습니다.' };
        }

        if (!deleteResponse.ok) {
            throw new Error(result.message || '링크 삭제에 실패했습니다.');
        }

        alert(result.message || '링크가 성공적으로 삭제되었습니다!');
        loadLinks();
    } catch (error) {
        console.error('링크 삭제 오류:', error);
        alert(`링크 삭제 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 채널 목록 로드 함수
async function loadChannels() {
    if (!checkAuthentication()) return;

    const channelTableBody = document.getElementById('channelTableBody');
    if (!channelTableBody) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Channel`);
        if (!response.ok) {
            throw new Error('채널 데이터를 가져오는데 실패했습니다.');
        }

        const channels = await response.json();

        channelTableBody.innerHTML = '';

        if (channels.length === 0) {
            channelTableBody.innerHTML = '<tr><td colspan="4">등록된 채널이 없습니다.</td></tr>';
            return;
        }

        channels.forEach((channel, index) => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${channel.name || ''}</td>
                <td>${channel.code || ''}</td>
                <td><a href="${channel.url || '#'}" target="_blank">${channel.url || ''}</a></td>
                <td>
                    <button class="action-btn action-btn-delete" onclick="deleteChannel('${channel.code}')">삭제</button>
                </td>
            `;
            channelTableBody.appendChild(row);
        });
    } catch (error) {
        console.error('채널 목록 로드 오류:', error);
        channelTableBody.innerHTML = `<tr><td colspan="4">채널 데이터를 불러오는데 문제가 발생했습니다: ${error.message}</td></tr>`;
    }
}

// 채널 폼 제출 처리 함수
async function handleChannelFormSubmit(event) {
    event.preventDefault();
    if (!checkAuthentication()) return;

    const channelCode = document.getElementById('channelCode').value;
    const channelName = document.getElementById('channelName').value;
    const channelUrl = document.getElementById('channelUrl').value;
    const channelChatContent = document.getElementById('channelChatContent').value;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Channel/Input`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                channel_code: channelCode,
                channel_name: channelName,
                channel_url: channelUrl,
                channel_chat_content: channelChatContent
            })
        });

        const result = await response.json();

        if (!response.ok) {
            throw new Error(result.message || '채널 저장에 실패했습니다.');
        }

        alert(result.message || '채널이 성공적으로 저장되었습니다!');
        
        document.getElementById('channelForm').reset();
        loadChannels();
    } catch (error) {
        console.error('채널 저장 오류:', error);
        alert(`채널 저장 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 채널 삭제 함수
async function deleteChannel(code) {
    if (!checkAuthentication()) return;
    
    if (!confirm(`정말로 이 채널을 삭제하시겠습니까?`)) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/Channel/${code}`, {
            method: 'DELETE'
        });
        
        const result = await response.json();
        
        if (!response.ok) {
            throw new Error(result.message || '채널 삭제에 실패했습니다.');
        }
        
        alert(result.message || '채널이 성공적으로 삭제되었습니다!');
        loadChannels();
    } catch (error) {
        console.error('채널 삭제 오류:', error);
        alert(`채널 삭제 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 미션 목록 로드 함수
async function loadMissions() {
    if (!checkAuthentication()) return;

    const missionTableBody = document.getElementById('missionTableBody');
    if (!missionTableBody) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Mission`);
        if (!response.ok) {
            throw new Error('미션 데이터를 가져오는데 실패했습니다.');
        }

        const missions = await response.json();      
        
        missionTableBody.innerHTML = '';

        if (missions.length === 0) {
            missionTableBody.innerHTML = '<tr><td colspan="4">등록된 미션이 없습니다.</td></tr>';
            return;
        }

        missions.forEach((mission, index) => {
            const typeValue = mission.mission_Type !== undefined ? mission.mission_Type : 
                            (mission.mission_type !== undefined ? mission.mission_type : 
                            (mission.missionType !== undefined ? mission.missionType : 
                            (mission.type !== undefined ? mission.type : -1)));

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${mission.missionName || mission.mission_Name || mission.mission_name || ''}</td>
                <td>${getMissionTypeText(typeValue)}</td>
                <td>${mission.mission_Rewords || mission.missionRewards || mission.mission_rewords || 0}</td>
                <td>
                    <button class="action-btn action-btn-delete" onclick="deleteMission(${index})">삭제</button>
                </td>
            `;
            missionTableBody.appendChild(row);
        });
    } catch (error) {
        console.error('미션 목록 로드 오류:', error);
        missionTableBody.innerHTML = `<tr><td colspan="4">미션 데이터를 불러오는데 문제가 발생했습니다: ${error.message}</td></tr>`;
    }
}

// 미션 타입 텍스트 변환 함수
function getMissionTypeText(missionType) {
    const typeValue = parseInt(missionType);
    
    switch (typeValue) {
        case 0: return '출석 미션';
        case 1: return '일반 미션';
        case 2: return '이벤트 미션';
        default: return '알 수 없음';
    }
}

// 미션 폼 제출 처리 함수
async function handleMissionFormSubmit(event) {
    event.preventDefault();
    if (!checkAuthentication()) return;

    const missionName = document.getElementById('missionName').value;
    const missionType = parseInt(document.getElementById('missionType').value);
    const missionRewards = document.getElementById('missionRewards').value;
    const missionChatContent = document.getElementById('missionChatContent').value;

    const missionTypeValue = getEnumValue(missionType);

    try {
        const response = await fetch(`${API_BASE_URL}/api/Mission`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                mission_name: missionName,
                mission_type: missionTypeValue,
                mission_rewords: parseInt(missionRewards),
                mission_chat_content: missionChatContent
            })
        });

        let result;
        try {
            result = await response.json();
        } catch (e) {
            result = { message: '응답을 처리하는 중 오류가 발생했습니다.' };
        }

        if (!response.ok) {
            throw new Error(result.message || '미션 저장에 실패했습니다.');
        }

        alert(result.message || '미션이 성공적으로 저장되었습니다!');

        document.getElementById('missionForm').reset();
        loadMissions();
    } catch (error) {
        console.error('미션 저장 오류:', error);
        alert(`미션 저장 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 미션 삭제 함수
async function deleteMission(index) {
    if (!checkAuthentication()) return;
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/Mission`);
        if (!response.ok) {
            throw new Error('미션 데이터를 가져오는데 실패했습니다.');
        }
        
        const missions = await response.json();
        
        if (index < 0 || index >= missions.length) {
            alert('유효하지 않은 미션 인덱스입니다.');
            return;
        }
        
        const mission = missions[index];
        const missionName = mission.mission_Name || mission.missionName || '';
        
        if (!missionName) {
            alert('미션 이름을 찾을 수 없습니다.');
            return;
        }
        
        if (!confirm(`정말로 "${missionName}" 미션을 삭제하시겠습니까?`)) {
            return;
        }
        
        const deleteResponse = await fetch(`${API_BASE_URL}/api/Mission/${encodeURIComponent(missionName)}`, {
            method: 'DELETE'
        });
        
        let result;
        const contentType = deleteResponse.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            result = await deleteResponse.json();
        } else {
            const text = await deleteResponse.text();
            result = { message: text || '응답이 없습니다.' };
        }
        
        if (!deleteResponse.ok) {
            throw new Error(result.message || '미션 삭제에 실패했습니다.');
        }
        
        alert(result.message || '미션이 성공적으로 삭제되었습니다!');
        loadMissions();
    } catch (error) {
        console.error('미션 삭제 오류:', error);
        alert(`미션 삭제 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 미션 타입을 정수로 변환하는 함수
function getEnumValue(typeValue) {
    if (!isNaN(parseInt(typeValue))) {
        return parseInt(typeValue);
    }
    
    switch(typeValue) {
        case 'Daily': return 0;
        case 'Mission': return 1;
        case 'Event': return 2;
        default: return 0;
    }
}

// 맞춤 응답 전송 및 저장 함수 (수정됨)
async function sendCustomResponse(chatId) {
    if (!checkAuthentication()) return;

    const inputElement = document.querySelector(`.custom-response-input[data-chat-id="${chatId}"]`);
    if (!inputElement) return;

    const customResponse = inputElement.value.trim();
    if (!customResponse) {
        alert('맞춤 응답 내용을 입력해주세요.');
        return;
    }

    try {
        const requestData = {
            ChatId: chatId,
            UserQuestion: customResponse
        };
        console.log('Sending request to:', `${API_BASE_URL}/api/Question/UserQuestion`);
        console.log('Request data:', requestData);
        // 1. 먼저 User_Question 필드에 저장
        const updateResponse = await fetch(`${API_BASE_URL}/api/Question/UserQuestion`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (!updateResponse.ok) {
            const errorText = await updateResponse.text();
            throw new Error(`사용자 응답 저장에 실패했습니다: ${errorText}`);
        }

        alert(`사용자에게 맞춤 응답을 저장했습니다.`);

        // 입력 필드는 비우지 않고 그대로 유지 (요청사항)
        // inputElement.value = ''; // 이 줄을 제거하거나 주석처리

        // 사용자 목록 새로고침 (User_Question이 업데이트되었으므로)
        loadUserList();

    } catch (error) {
        console.error('맞춤 응답 처리 오류:', error);
        alert(`맞춤 응답 처리 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 봇 메시지 로드 함수
async function loadBotMessages() {
    if (!checkAuthentication()) return;

    const welcomeMessage = document.getElementById('welcomeMessage');
    const questionMessage = document.getElementById('questionMessage');
    const botMessageInfo = document.getElementById('botMessageInfo');

    if (!welcomeMessage || !questionMessage || !botMessageInfo) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/BotMessage`);
        if (!response.ok) {
            throw new Error('봇 메시지 데이터를 가져오는데 실패했습니다.');
        }

        const botMessages = await response.json();
        
        welcomeMessage.value = botMessages.welcome_message || '';
        questionMessage.value = botMessages.question_message || '';
        
        botMessageInfo.innerHTML = `
            <div class="info-card">
                <h4>현재 설정된 메시지</h4>
                <div class="message-preview">
                    <h5>시작 메시지 (/start)</h5>
                    <div class="preview-content">${botMessages.welcome_message || '설정된 메시지가 없습니다.'}</div>
                </div>
                <div class="message-preview">
                    <h5>"나" 버튼 응답 메시지</h5>
                    <div class="preview-content">${botMessages.question_message || '설정된 메시지가 없습니다.'}</div>
                </div>
            </div>
        `;
    } catch (error) {
        console.error('봇 메시지 로드 오류:', error);
        botMessageInfo.innerHTML = `<p class="error-message">봇 메시지를 불러오는데 문제가 발생했습니다: ${error.message}</p>`;
    }
}

// 봇 메시지 폼 제출 처리 함수
async function handleBotMessageFormSubmit(event) {
    event.preventDefault();
    if (!checkAuthentication()) return;

    const welcomeMessage = document.getElementById('welcomeMessage').value;
    const questionMessage = document.getElementById('questionMessage').value;

    try {
        const response = await fetch(`${API_BASE_URL}/api/BotMessage`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                welcome_message: welcomeMessage,
                question_message: questionMessage
            })
        });

        const result = await response.json();

        if (!response.ok) {
            throw new Error(result.message || '봇 메시지 저장에 실패했습니다.');
        }

        alert(result.message || '봇 메시지가 성공적으로 저장되었습니다!');
        loadBotMessages();
    } catch (error) {
        console.error('봇 메시지 저장 오류:', error);
        alert(`봇 메시지 저장 중 문제가 발생했습니다: ${error.message}`);
    }
}

// 채팅 목록 로드 함수 (완전히 수정됨 - 선택 상태 안정적 보존)
async function loadChatList() {
    if (!checkAuthentication()) return;

    const chatUserList = document.getElementById('chatUserList');
    if (!chatUserList) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Livechat/Users`);
        if (!response.ok) {
            throw new Error('채팅 사용자 목록을 가져오는데 실패했습니다.');
        }

        const chatUsers = await response.json();

        chatUserList.innerHTML = '';

        if (chatUsers.length === 0) {
            chatUserList.innerHTML = '<li class="no-chats">대화 중인 사용자가 없습니다.</li>';
            return;
        }

        chatUsers.forEach(user => {
            const listItem = document.createElement('li');
            listItem.setAttribute('data-chat-id', user.chatId);

            // 읽지 않은 메시지가 있으면 표시 (현재 선택된 사용자가 아닌 경우만)
            if (user.unreadCount > 0 && user.chatId !== currentSelectedChatId) {
                listItem.classList.add('unread-message');
            }

            // 현재 선택된 사용자라면 active 클래스 추가
            if (currentSelectedChatId && user.chatId === currentSelectedChatId) {
                listItem.classList.add('active');
            }

            const lastTime = new Date(user.lastTimestamp);
            const timeString = `${lastTime.getHours()}:${lastTime.getMinutes().toString().padStart(2, '0')}`;

            listItem.innerHTML = `
                <div class="chat-user-name">${user.username}</div>
                <div class="chat-last-message">${user.lastMessage}</div>
                <div class="chat-time">${timeString}</div>
            `;

            listItem.addEventListener('click', () => {
                document.querySelectorAll('#chatUserList li').forEach(li => {
                    li.classList.remove('active');
                });

                listItem.classList.remove('unread-message');
                listItem.classList.add('active');
                loadChatMessages(user.chatId);
            });

            chatUserList.appendChild(listItem);
        });
    } catch (error) {
        console.error('채팅 사용자 목록 로드 오류:', error);
        chatUserList.innerHTML = `<li class="error">사용자 목록을 불러오는데 문제가 발생했습니다: ${error.message}</li>`;
    }
}

// 개선된 주기적 채팅 목록 갱신 (완전히 수정됨)
setInterval(() => {
    if (document.getElementById('chatTab') && document.getElementById('chatTab').classList.contains('active')) {
        const now = Date.now();
        
        // 선택된 사용자가 있으면 갱신 빈도를 줄임 (1초), 없으면 자주 갱신 (0.5초)
        const updateInterval = currentSelectedChatId ? 1000 : 500;
        
        if (now - lastChatListUpdate > updateInterval) {
            loadChatList();
            lastChatListUpdate = now;
        }
    }
}, 1000); // 1초마다 조건 확인하되, 실제 갱신은 조건에 따라

// ✨ 추가 최적화: 이중 폴링 시스템 (현재 선택된 대화 전용 초고속 갱신)
setInterval(() => {
    if (currentSelectedChatId && 
        document.getElementById('chatTab') && 
        document.getElementById('chatTab').classList.contains('active')) {
        
        // 현재 선택된 대화만 초고속 갱신 (0.3초마다)
        fetch(`${API_BASE_URL}/api/Livechat/${currentSelectedChatId}`)
            .then(response => response.ok ? response.json() : null)
            .then(messages => {
                if (messages) {
                    updateMessagesIfNeeded(messages);
                }
            })
            .catch(() => {}); // 에러는 무시 (메인 폴링에서 처리)
    }
}, 300); // 0.3초마다 실행하여 극도의 실시간성 확보

// ==================== 설정 탭 관련 기능들 ====================

// 설정 탭 초기화 함수
function initializeSettingsTab() {
    loadBotTokens();
    loadAdminList();    
    console.log('설정 탭이 초기화되었습니다.');
}

// ChatBot 토큰 저장 함수
async function handleChatBotTokenFormSubmit(event) {
    event.preventDefault();

    const chatBotToken = document.getElementById('chatBotToken').value.trim();

    if (!chatBotToken) {
        alert('ChatBot 토큰을 입력해주세요.');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/Bot/chatbot`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                chatBotToken: chatBotToken
            })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                alert('ChatBot 토큰이 성공적으로 저장되었습니다.');
                // 입력 필드는 비우지 않고 토큰 정보만 새로고침
                loadBotTokens();
            } else {
                alert('ChatBot 토큰 저장에 실패했습니다: ' + (result.message || '알 수 없는 오류'));
            }
        } else {
            const errorResult = await response.json();
            alert('ChatBot 토큰 저장에 실패했습니다: ' + (errorResult.message || response.statusText));
        }
    } catch (error) {
        console.error('ChatBot 토큰 저장 오류:', error);
        alert('ChatBot 토큰 저장 중 오류가 발생했습니다: ' + error.message);
    }
}

// MainBot 토큰 저장 함수
async function handleMainBotTokenFormSubmit(event) {
    event.preventDefault();

    const mainBotToken = document.getElementById('mainBotToken').value.trim();

    if (!mainBotToken) {
        alert('MainBot 토큰을 입력해주세요.');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/Bot/mainbot`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                mainBotToken: mainBotToken
            })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                alert('MainBot 토큰이 성공적으로 저장되었습니다.');
                // 입력 필드는 비우지 않고 토큰 정보만 새로고침
                loadBotTokens();
            } else {
                alert('MainBot 토큰 저장에 실패했습니다: ' + (result.message || '알 수 없는 오류'));
            }
        } else {
            const errorResult = await response.json();
            alert('MainBot 토큰 저장에 실패했습니다: ' + (errorResult.message || response.statusText));
        }
    } catch (error) {
        console.error('MainBot 토큰 저장 오류:', error);
        alert('MainBot 토큰 저장 중 오류가 발생했습니다: ' + error.message);
    }
}
// 관리자 계정 폼 제출 처리
async function handleAdminAccountFormSubmit(event) {
    event.preventDefault();
    
    const username = document.getElementById('adminUsername').value.trim();
    const password = document.getElementById('adminPassword').value.trim();
    
    if (!username || !password) {
        alert('모든 필드를 입력해주세요.');
        return;
    }
    
    // 유효성 검사
    const usernamePattern = /^[a-zA-Z0-9_]{1,20}$/;
    if (!usernamePattern.test(username)) {
        alert('아이디는 1-20자의 영문, 숫자, 언더스코어만 사용 가능합니다.');
        return;
    }
    
    if (password.length < 1) {
        alert('비밀번호를 입력해주세요.');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/Account/Input`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Id: username,
                Pw: password,
            })
        });
        
        if (response.ok) {
            const result = await response.json();
            if (result && result.id) {
                alert('관리자 계정이 성공적으로 생성되었습니다.');
                event.target.reset(); // 폼 초기화
                loadAdminList(); // 관리자 목록 새로고침
            } else {
                alert('계정 생성에 실패했습니다: ' + (result.message || '알 수 없는 오류'));
            }
        } else {
            const errorResult = await response.json();
            alert('계정 생성에 실패했습니다: ' + (errorResult.message || response.statusText));
        }
    } catch (error) {
        console.error('관리자 계정 생성 오류:', error);
        alert('계정 생성 중 오류가 발생했습니다: ' + error.message);
    }
}

// 관리자 목록 로드
async function loadAdminList() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/Account`);
        
        if (response.ok) {
            const admins = await response.json();
            displayAdminList(admins);
        } else {
            console.error('관리자 목록 로드 실패:', response.statusText);
            displayAdminList([]);
        }
    } catch (error) {
        console.error('관리자 목록 로드 오류:', error);
        displayAdminList([]);
    }
}

// 관리자 목록 표시
function displayAdminList(admins) {
    const tableBody = document.getElementById('adminTableBody');
    if (!tableBody) return;
    
    if (!admins || admins.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="5" style="text-align: center; color: #888; padding: 20px;">
                    등록된 관리자가 없습니다.
                </td>
            </tr>
        `;
        return;
    }
    
    tableBody.innerHTML = admins.map(admin => `
        <tr>
            <td>${escapeHtml(admin.id)}</td>
            <td>${escapeHtml(admin.pw)}</td>
            <td>
                <button onclick="deleteAdmin('${admin.id}', '${escapeHtml(admin.id)}')" class="action-btn action-btn-delete">삭제</button>
            </td>
        </tr>
    `).join('');
}

// HTML 이스케이프 함수
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
async function loadBotTokens() {
    if (!checkAuthentication()) return;

    const chatBotTokenInput = document.getElementById('chatBotToken');
    const mainBotTokenInput = document.getElementById('mainBotToken');
    const botTokenInfo = document.getElementById('botTokenInfo');

    if (!chatBotTokenInput || !mainBotTokenInput || !botTokenInfo) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/Bot/tokens`);
        if (!response.ok) {
            throw new Error('봇 토큰 데이터를 가져오는데 실패했습니다.');
        }

        const botTokens = await response.json();
        
        // 입력 필드는 비우기
        chatBotTokenInput.value = '';
        mainBotTokenInput.value = '';
        
        // 토큰 정보 미리보기 표시 (마스킹 제거, 파란색 강조)
        botTokenInfo.innerHTML = `
            <div class="info-card">
                <h4>현재 설정된 봇 토큰</h4>
                <div class="token-preview" style="border-left: 4px solid #3a80ff; padding-left: 15px;">
                    <h5 style="color: #3a80ff; margin: 0 0 10px 0;">ChatBot 토큰</h5>
                    <div class="preview-content token-display">${botTokens.chat_bot_token || '설정된 토큰이 없습니다.'}</div>
                </div>
                <div class="token-preview" style="border-left: 4px solid #3a80ff; padding-left: 15px; margin-top: 20px;">
                    <h5 style="color: #3a80ff; margin: 0 0 10px 0;">MainBot 토큰</h5>
                    <div class="preview-content token-display">${botTokens.main_bot_token || '설정된 토큰이 없습니다.'}</div>
                </div>
            </div>
        `;
    } catch (error) {
        console.error('봇 토큰 로드 오류:', error);
        botTokenInfo.innerHTML = `<p class="error-message">봇 토큰을 불러오는데 문제가 발생했습니다: ${error.message}</p>`;
    }
}

// 관리자 삭제
async function deleteAdmin(adminId, username) {
    if (!confirm(`정말로 관리자 '${username}'를 삭제하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다.`)) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/Account/${adminId}`, {
            method: 'DELETE'
        });
        
        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                alert('관리자가 성공적으로 삭제되었습니다.');
                loadAdminList(); // 목록 새로고침
            } else {
                alert('삭제에 실패했습니다: ' + (result.message || '알 수 없는 오류'));
            }
        } else {
            const errorResult = await response.json();
            alert('삭제에 실패했습니다: ' + (errorResult.message || response.statusText));
        }
    } catch (error) {
        console.error('관리자 삭제 오류:', error);
        alert('삭제 중 오류가 발생했습니다: ' + error.message);
    }
}
// 전역 함수로 설정 (HTML onclick에서 사용)
window.deleteAdmin = deleteAdmin;