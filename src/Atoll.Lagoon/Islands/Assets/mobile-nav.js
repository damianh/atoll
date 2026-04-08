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
    const wrapper = document.getElementById('sidebar-wrapper');
    const close = element.querySelector('#mobile-nav-close');

    if (!toggle || !menu || !wrapper) return;

    // Create a backdrop overlay for tap-outside-to-close
    const backdrop = document.createElement('div');
    backdrop.className = 'mobile-nav-backdrop';
    backdrop.setAttribute('aria-hidden', 'true');
    document.body.appendChild(backdrop);

    function isOpen() {
        return wrapper.getAttribute('aria-hidden') === 'false';
    }

    function open() {
        wrapper.setAttribute('aria-hidden', 'false');
        menu.setAttribute('aria-hidden', 'false');
        backdrop.setAttribute('aria-hidden', 'false');
        document.body.classList.add(OPEN_CLASS);
        toggle.setAttribute('aria-expanded', 'true');
        trapFocus(menu);
        close && close.focus();
    }

    function closeMenu() {
        wrapper.setAttribute('aria-hidden', 'true');
        menu.setAttribute('aria-hidden', 'true');
        backdrop.setAttribute('aria-hidden', 'true');
        document.body.classList.remove(OPEN_CLASS);
        toggle.setAttribute('aria-expanded', 'false');
        toggle.focus();
    }

    toggle.addEventListener('click', function () {
        if (isOpen()) {
            closeMenu();
        } else {
            open();
        }
    });
    close && close.addEventListener('click', closeMenu);
    backdrop.addEventListener('click', closeMenu);

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && isOpen()) {
            closeMenu();
        }
    });
}
