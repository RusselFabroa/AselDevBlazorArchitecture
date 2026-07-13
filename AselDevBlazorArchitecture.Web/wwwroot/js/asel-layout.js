// asel-layout.js
// Place in: AselDevBlazorArchitecture.Web/wwwroot/js/asel-layout.js
// Reference in index.html / _Host.cshtml:
//   <script src="js/asel-layout.js"></script>

window.aselLayout = (() => {
    const MOBILE_BREAKPOINT = 960;
    let dotNetRef   = null;
    let resizeTimer = null;

    function isMobile() {
        return window.innerWidth <= MOBILE_BREAKPOINT;
    }

    function notifyBlazor() {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('SetMobileMode', isMobile());
        }
    }

    function onResize() {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(notifyBlazor, 80);
    }

    return {
        init(ref) {
            dotNetRef = ref;
            window.addEventListener('resize', onResize);
            // Fire immediately so Blazor knows the initial state
            notifyBlazor();
        },
        dispose() {
            window.removeEventListener('resize', onResize);
            dotNetRef = null;
        }
    };
})();



// Browser session helpers. Authentication is stored only in an HttpOnly cookie;
// JavaScript never receives or persists the session credential.
window.aselSession = {
    async signIn(request) {
        const response = await fetch('/api/browser-session/sign-in', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });

        return await response.json();
    },

    async signOut() {
        await fetch('/api/browser-session/sign-out', {
            method: 'POST',
            credentials: 'same-origin'
        });
    }
};
