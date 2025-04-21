async function fetchIPAddress() {
    try {
        const response = await fetch('https://myip.nobita.cloud/api/v4');
        const data = await response.json();
        document.getElementById('ip_login').textContent = "Last login address: " + data.ip;
    } catch (error) {
        document.getElementById('ip_login').textContent = "getting fail";
    }
}

function checkDomain() {
    const currentUrl = window.location.href;
    const allowedDomains = ['localhost', '192.168.1.91', 'thuyto.nvtai.id.vn'];

    const isAllowed = allowedDomains.some(domain => currentUrl.includes(domain));

    if (!isAllowed) {
        window.location.href = '/error/403';
    }
}

window.onload = function() {
    fetchIPAddress();
    checkDomain();
};