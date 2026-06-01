(function () {
  'use strict';

  var deck = document.getElementById('swell-deck');
  if (!deck) return;

  var slides = Array.from(deck.querySelectorAll('.swell-slide'));
  var total = slides.length;
  if (total === 0) return;

  var current = 0;
  var overviewActive = false;
  var transitioning = false;

  // --------------------------------------------------------------------------
  // Initialise from URL hash
  // --------------------------------------------------------------------------
  function indexFromHash() {
    var hash = window.location.hash;
    var match = hash.match(/^#\/(\d+)$/);
    if (match) {
      var idx = parseInt(match[1], 10);
      if (idx >= 0 && idx < total) return idx;
    }
    return 0;
  }

  // --------------------------------------------------------------------------
  // Slide display
  // --------------------------------------------------------------------------
  function showSlide(idx, direction) {
    if (idx === current && !transitioning) return;
    if (idx < 0 || idx >= total) return;

    var prevSlide = slides[current];
    var nextSlide = slides[idx];

    // Determine effective transition
    var transition = nextSlide.dataset.transition
      || deck.dataset.defaultTransition
      || 'none';

    if (transition !== 'none' && !overviewActive) {
      transitioning = true;
      deck.setAttribute('data-transitioning', '');
      deck.setAttribute('data-transition', transition);

      prevSlide.classList.add('swell-leaving');
      nextSlide.classList.add('swell-entering');
      nextSlide.setAttribute('aria-hidden', 'false');
      nextSlide.classList.add('swell-active');

      var duration = parseFloat(
        getComputedStyle(deck).getPropertyValue('--swell-transition-duration')
      ) || 400;

      setTimeout(function () {
        prevSlide.setAttribute('aria-hidden', 'true');
        prevSlide.classList.remove('swell-active', 'swell-leaving');
        nextSlide.classList.remove('swell-entering');
        deck.removeAttribute('data-transitioning');
        deck.removeAttribute('data-transition');
        transitioning = false;
      }, duration);
    } else {
      prevSlide.setAttribute('aria-hidden', 'true');
      prevSlide.classList.remove('swell-active');
      nextSlide.setAttribute('aria-hidden', 'false');
      nextSlide.classList.add('swell-active');
    }

    current = idx;
    updateSlideNumbers();
    updateNavbarCounter();
    updateHash();
    announceSlide();

    // Clear click-reveal state on new slide
    clearClickReveal(nextSlide);
    notifyPresenter();

    // Notify drawing overlay
    document.dispatchEvent(new CustomEvent('swell-slide-change', { detail: { slide: idx } }));
  }

  // --------------------------------------------------------------------------
  // Slide numbers
  // --------------------------------------------------------------------------
  function updateSlideNumbers() {
    slides.forEach(function (slide, idx) {
      var num = slide.querySelector('.swell-slide-number');
      if (num) {
        num.textContent = (idx + 1) + ' / ' + total;
      }
    });
  }

  // --------------------------------------------------------------------------
  // URL hash
  // --------------------------------------------------------------------------
  function updateHash() {
    var hash = '#/' + current;
    if (window.location.hash !== hash) {
      history.replaceState(null, '', hash);
    }
  }

  // --------------------------------------------------------------------------
  // Live region for screen reader announcements
  // --------------------------------------------------------------------------
  var liveRegion = document.createElement('div');
  liveRegion.setAttribute('aria-live', 'polite');
  liveRegion.setAttribute('aria-atomic', 'true');
  liveRegion.className = 'swell-sr-only';
  liveRegion.style.cssText = 'position:absolute;left:-9999px;width:1px;height:1px;overflow:hidden';
  document.body.appendChild(liveRegion);

  function announceSlide() {
    liveRegion.textContent = 'Slide ' + (current + 1) + ' of ' + total;
  }

  // --------------------------------------------------------------------------
  // Click-reveal (:::Click blocks)
  // --------------------------------------------------------------------------
  function getClickBlocks(slide) {
    return Array.from(slide.querySelectorAll('.swell-click'));
  }

  var clickState = {}; // slideIndex -> number of revealed blocks

  function clearClickReveal(slide) {
    var idx = slides.indexOf(slide);
    clickState[idx] = 0;
    getClickBlocks(slide).forEach(function (el) {
      el.classList.remove('swell-click-visible');
    });
  }

  // Returns true if a click block was revealed (consumed the keypress), false otherwise.
  function revealNextClick(slideIdx) {
    var slide = slides[slideIdx];
    var blocks = getClickBlocks(slide);
    var revealed = clickState[slideIdx] || 0;
    if (revealed < blocks.length) {
      blocks[revealed].classList.add('swell-click-visible');
      clickState[slideIdx] = revealed + 1;
      notifyPresenter();
      return true;
    }
    return false;
  }

  // Returns true if a click block was hidden (consumed the keypress).
  function hideLastClick(slideIdx) {
    var slide = slides[slideIdx];
    var blocks = getClickBlocks(slide);
    var revealed = clickState[slideIdx] || 0;
    if (revealed > 0) {
      blocks[revealed - 1].classList.remove('swell-click-visible');
      clickState[slideIdx] = revealed - 1;
      notifyPresenter();
      return true;
    }
    return false;
  }

  // --------------------------------------------------------------------------
  // Navigation
  // --------------------------------------------------------------------------
  function goNext() {
    if (overviewActive) return;
    if (revealNextClick(current)) return;
    showSlide(current + 1, 'forward');
  }

  function goPrev() {
    if (overviewActive) return;
    if (hideLastClick(current)) return;
    showSlide(current - 1, 'backward');
  }

  function goTo(idx) {
    showSlide(idx, idx > current ? 'forward' : 'backward');
  }

  // --------------------------------------------------------------------------
  // Overview mode
  // --------------------------------------------------------------------------
  function toggleOverview() {
    overviewActive = !overviewActive;
    if (overviewActive) {
      deck.classList.add('swell-overview');
      deck.style.overflow = 'auto';
      deck.style.maxHeight = '100vh';
      slides.forEach(function (s) {
        s.style.display = 'block';
        s.tabIndex = 0;
        s.setAttribute('role', 'button');
      });
    } else {
      deck.classList.remove('swell-overview');
      deck.style.overflow = '';
      deck.style.maxHeight = '';
      slides.forEach(function (s) {
        s.style.display = '';
        s.removeAttribute('tabIndex');
        s.removeAttribute('role');
      });
      // Restore correct active slide
      slides.forEach(function (s, idx) {
        if (idx === current) {
          s.setAttribute('aria-hidden', 'false');
          s.classList.add('swell-active');
        } else {
          s.setAttribute('aria-hidden', 'true');
          s.classList.remove('swell-active');
        }
      });
    }
  }

  // --------------------------------------------------------------------------
  // Fullscreen
  // --------------------------------------------------------------------------
  function toggleFullscreen() {
    if (!document.fullscreenElement) {
      deck.requestFullscreen && deck.requestFullscreen();
    } else {
      document.exitFullscreen && document.exitFullscreen();
    }
  }

  // --------------------------------------------------------------------------
  // Presenter mode (BroadcastChannel)
  // --------------------------------------------------------------------------
  var presenterChannel = null;

  function openPresenter() {
    var presenterUrl = window.location.pathname + '?swell-presenter=1' + window.location.hash;
    window.open(presenterUrl, 'swell-presenter', 'width=1024,height=640');
  }

  function initBroadcastChannel() {
    if (typeof BroadcastChannel === 'undefined') return;
    presenterChannel = new BroadcastChannel('swell-presenter');
    presenterChannel.onmessage = function (evt) {
      if (evt.data && typeof evt.data.slide === 'number') {
        if (evt.data.slide !== current) {
          showSlide(evt.data.slide, evt.data.slide > current ? 'forward' : 'backward');
        }
      }
    };
  }

  function notifyPresenter() {
    if (presenterChannel) {
      presenterChannel.postMessage({
        slide: current,
        clicks: clickState[current] || 0,
        total: total
      });
    }
  }

  // --------------------------------------------------------------------------
  // Touch / swipe
  // --------------------------------------------------------------------------
  var touchStartX = 0;
  var touchStartY = 0;
  var swipeThreshold = 50;

  deck.addEventListener('touchstart', function (e) {
    touchStartX = e.touches[0].clientX;
    touchStartY = e.touches[0].clientY;
  }, { passive: true });

  deck.addEventListener('touchend', function (e) {
    var dx = e.changedTouches[0].clientX - touchStartX;
    var dy = e.changedTouches[0].clientY - touchStartY;
    if (Math.abs(dx) > Math.abs(dy) && Math.abs(dx) > swipeThreshold) {
      if (dx < 0) { goNext(); } else { goPrev(); }
    }
  }, { passive: true });

  // --------------------------------------------------------------------------
  // Keyboard
  // --------------------------------------------------------------------------
  document.addEventListener('keydown', function (e) {
    if (e.target && (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA')) return;

    switch (e.key) {
      case 'ArrowRight':
      case 'ArrowDown':
      case ' ':
      case 'PageDown':
        e.preventDefault();
        goNext();
        break;
      case 'ArrowLeft':
      case 'ArrowUp':
      case 'PageUp':
        e.preventDefault();
        goPrev();
        break;
      case 'Home':
        e.preventDefault();
        goTo(0);
        break;
      case 'End':
        e.preventDefault();
        goTo(total - 1);
        break;
      case 'o':
      case 'O':
        toggleOverview();
        break;
      case 'f':
      case 'F':
        toggleFullscreen();
        break;
      case 'p':
      case 'P':
        openPresenter();
        break;
      case 'Escape':
        if (overviewActive) toggleOverview();
        break;
    }
  });

  // --------------------------------------------------------------------------
  // Overview click handler
  // --------------------------------------------------------------------------
  deck.addEventListener('click', function (e) {
    if (!overviewActive) return;
    var slide = e.target.closest('.swell-slide');
    if (slide) {
      var idx = slides.indexOf(slide);
      if (idx >= 0) {
        toggleOverview();
        goTo(idx);
      }
    }
  });

  // --------------------------------------------------------------------------
  // popstate (browser back/forward)
  // --------------------------------------------------------------------------
  window.addEventListener('popstate', function () {
    goTo(indexFromHash());
  });

  // --------------------------------------------------------------------------
  // Navigation bar
  // --------------------------------------------------------------------------
  var navbarCounter = null;
  var drawingBtn = null;

  function initNavbar() {
    var navbar = document.getElementById('swell-navbar');
    if (!navbar) return;

    navbarCounter = navbar.querySelector('[data-swell-counter]');
    drawingBtn = navbar.querySelector('[data-swell-action="drawing"]');

    navbar.addEventListener('click', function (e) {
      var btn = e.target.closest('[data-swell-action]');
      if (!btn) return;
      var action = btn.dataset.swellAction;
      switch (action) {
        case 'prev': goPrev(); break;
        case 'next': goNext(); break;
        case 'overview': toggleOverview(); break;
        case 'fullscreen': toggleFullscreen(); break;
        case 'presenter': openPresenter(); break;
        case 'drawing':
          // Dispatch a synthetic 'd' keydown to toggle drawing via swell-drawing.js
          document.dispatchEvent(new KeyboardEvent('keydown', { key: 'd', bubbles: true }));
          btn.classList.toggle('swell-navbar-active');
          break;
      }
    });
  }

  function updateNavbarCounter() {
    if (navbarCounter) {
      navbarCounter.textContent = (current + 1) + ' / ' + total;
    }
  }

  // --------------------------------------------------------------------------
  // Initialise
  // --------------------------------------------------------------------------
  initBroadcastChannel();
  initNavbar();
  initNavbar();

  var startIndex = indexFromHash();
  slides.forEach(function (s, idx) {
    s.setAttribute('aria-hidden', idx === startIndex ? 'false' : 'true');
    if (idx === startIndex) s.classList.add('swell-active');
  });
  current = startIndex;
  updateSlideNumbers();
  updateNavbarCounter();
  updateHash();
  announceSlide();

})();
