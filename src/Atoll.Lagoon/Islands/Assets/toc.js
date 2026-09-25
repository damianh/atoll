(function () {
  var toc = document.querySelector('.docs-toc nav');
  if (!toc) return;
  var links = Array.from(toc.querySelectorAll('a[href^="#"]'));
  if (!links.length) return;
  var headings = [];
  for (var i = 0; i < links.length; i++) {
    var el = document.getElementById(links[i].getAttribute('href').slice(1));
    if (el) headings.push({ el: el, link: links[i] });
  }
  if (!headings.length) return;
  var active = null;
  function setCurrent(entry) {
    if (active === entry) return;
    if (active) active.link.removeAttribute('aria-current');
    active = entry;
    if (active) active.link.setAttribute('aria-current', 'true');
  }
  var offset = parseFloat(getComputedStyle(document.documentElement)
    .getPropertyValue('--docs-header-height')) || 56;
  // Convert rem to px
  offset = offset * parseFloat(getComputedStyle(document.documentElement).fontSize) + 16;
  function onScroll() {
    // At the bottom of the page, activate the last heading
    if (window.innerHeight + window.scrollY >= document.body.scrollHeight - 2) {
      setCurrent(headings[headings.length - 1]);
      return;
    }
    var best = headings[0];
    for (var i = 0; i < headings.length; i++) {
      // +1px tolerance: anchor scrolling rounds scrollY while heading
      // geometry can stay fractional (e.g. 72.0625px vs a 72px offset)
      if (headings[i].el.getBoundingClientRect().top <= offset + 1) {
        best = headings[i];
      } else {
        break;
      }
    }
    setCurrent(best);
  }
  var ticking = false;
  window.addEventListener('scroll', function () {
    if (!ticking) {
      ticking = true;
      requestAnimationFrame(function () { onScroll(); ticking = false; });
    }
  }, { passive: true });
  onScroll();
})();
