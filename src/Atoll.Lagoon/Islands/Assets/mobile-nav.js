/**
 * Atoll Docs — Mobile Navigation
 *
 * Renders a hamburger button (visible on mobile via CSS) that toggles
 * a full-screen overlay sidebar. Traps focus when open and closes on Escape.
 */

const OPEN_CLASS = 'mobile-nav-open';

function trapFocus(element) {
    const focusable = element.querySelectorAll(
        'a[href], button:not([disabled]), input, [tabindex]:not([tabindex="-1"])'
    );
    const first = focusable[0];
    const last = focusable[focusable.length - 1];

    element.addEventListener('keydown', function onKeyDown(e) {
        if (e.key !== 'Tab') return;
        if (e.shiftKey) {
            if (document.activeElement === first) {
                e.preventDefault();
                last.focus();
            }
        } else {
            if (document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        }
    });
}

export default function init(element) {
    const toggle = element.querySelector('#mobile-nav-toggle');
    const menu = document.getElementById('mobile-nav-menu');
    const close = element.querySelector('#mobile-nav-close');

    if (!toggle || !menu) return;

    function open() {
        menu.style.display = 'block';
        document.body.classList.add(OPEN_CLASS);
        toggle.setAttribute('aria-expanded', 'true');
        menu.setAttribute('aria-hidden', 'false');
        trapFocus(menu);
        close && close.focus();
    }

    function closeMenu() {
        menu.style.display = 'none';
        document.body.classList.remove(OPEN_CLASS);
        toggle.setAttribute('aria-expanded', 'false');
        menu.setAttribute('aria-hidden', 'true');
        toggle.focus();
    }

    toggle.addEventListener('click', open);
    close && close.addEventListener('click', closeMenu);

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && menu.style.display === 'block') {
            closeMenu();
        }
    });
}
