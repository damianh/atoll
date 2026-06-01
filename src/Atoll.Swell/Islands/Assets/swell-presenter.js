(function () {
  'use strict';

  // Determine if this window is the presenter window or a regular slide window.
  var isPresenterWindow = window.location.search.indexOf('swell-presenter=1') >= 0;
  if (!isPresenterWindow) return;

  // ── DOM refs ──────────────────────────────────────────────────────────────
  var currentSlideEl  = document.getElementById('swell-presenter-current');
  var nextSlideEl     = document.getElementById('swell-presenter-next');
  var notesEl         = document.getElementById('swell-presenter-notes');
  var slideCounterEl  = document.getElementById('swell-presenter-counter');
  var elapsedEl       = document.getElementById('swell-presenter-elapsed');
  var clockEl         = document.getElementById('swell-presenter-clock');

  if (!currentSlideEl) return; // Not on the presenter page layout.

  // ── Slide data injected server-side ────────────────────────────────────────
  // Expected global: window.swellSlides = [{ notes: "..." }, ...]
  var slideNotes = (window.swellSlides || []).map(function (s) { return s.notes || ''; });
  var total = slideNotes.length;

  // ── BroadcastChannel ───────────────────────────────────────────────────────
  var channel = typeof BroadcastChannel !== 'undefined'
    ? new BroadcastChannel('swell-presenter')
    : null;

  var currentIndex = 0;

  function updatePresenterView(index, clickCount) {
    currentIndex = index;

    // Update slide counter
    if (slideCounterEl) {
      slideCounterEl.textContent = (index + 1) + ' / ' + total;
    }

    // Update notes
    if (notesEl) {
      notesEl.innerHTML = slideNotes[index] || '<em>No notes for this slide.</em>';
    }

    // Update current slide iframe
    if (currentSlideEl) {
      var base = window.location.pathname.replace('?swell-presenter=1', '');
      currentSlideEl.src = base + '#/' + index;
    }

    // Update next slide iframe
    if (nextSlideEl) {
      var nextIndex = index + 1 < total ? index + 1 : index;
      var base2 = window.location.pathname.replace('?swell-presenter=1', '');
      nextSlideEl.src = base2 + '#/' + nextIndex;
    }
  }

  // ── Receive messages from main window ─────────────────────────────────────
  if (channel) {
    channel.onmessage = function (evt) {
      if (evt.data && typeof evt.data.slide === 'number') {
        updatePresenterView(evt.data.slide, evt.data.clicks || 0);
      }
    };
  }

  // ── Keyboard navigation from presenter window ─────────────────────────────
  document.addEventListener('keydown', function (e) {
    if (!channel) return;
    var next = currentIndex;
    if (e.key === 'ArrowRight' || e.key === 'ArrowDown' || e.key === ' ' || e.key === 'PageDown') {
      next = Math.min(currentIndex + 1, total - 1);
    } else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp' || e.key === 'PageUp') {
      next = Math.max(currentIndex - 1, 0);
    } else {
      return;
    }
    e.preventDefault();
    if (next !== currentIndex) {
      channel.postMessage({ slide: next, clicks: 0, total: total, source: 'presenter' });
      updatePresenterView(next, 0);
    }
  });

  // ── Elapsed timer ─────────────────────────────────────────────────────────
  var startTime = Date.now();

  function padTwo(n) { return n < 10 ? '0' + n : String(n); }

  function updateClock() {
    var now = new Date();
    if (clockEl) {
      clockEl.textContent = padTwo(now.getHours()) + ':' + padTwo(now.getMinutes()) + ':' + padTwo(now.getSeconds());
    }
    var elapsed = Math.floor((Date.now() - startTime) / 1000);
    var h = Math.floor(elapsed / 3600);
    var m = Math.floor((elapsed % 3600) / 60);
    var s = elapsed % 60;
    if (elapsedEl) {
      elapsedEl.textContent = (h > 0 ? padTwo(h) + ':' : '') + padTwo(m) + ':' + padTwo(s);
    }
  }

  setInterval(updateClock, 1000);
  updateClock();

  // ── Initial view ─────────────────────────────────────────────────────────
  var hash = window.location.hash;
  var match = hash.match(/^#\/(\d+)$/);
  var initial = match ? parseInt(match[1], 10) : 0;
  updatePresenterView(initial, 0);

  // Announce to the main window which slide we're on.
  if (channel) {
    channel.postMessage({ slide: initial, clicks: 0, total: total, source: 'presenter' });
  }

})();
