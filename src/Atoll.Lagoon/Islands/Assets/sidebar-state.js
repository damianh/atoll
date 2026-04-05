(function () {
  try {
    if (window.matchMedia('(max-width:768px)').matches) return;

    var KEY = 'atoll:sidebar-state';

    function getState() {
      var nav = document.querySelector('.docs-sidebar nav[data-hash]');
      if (!nav) return null;
      var groups = nav.querySelectorAll('details[data-index]');
      var open = new Array(groups.length);
      for (var j = 0; j < groups.length; j++) {
        open[parseInt(groups[j].dataset.index, 10)] = groups[j].open;
      }
      var sidebar = document.querySelector('.docs-sidebar');
      return {
        hash: nav.getAttribute('data-hash'),
        open: open,
        scroll: sidebar ? sidebar.scrollTop : 0
      };
    }

    function save() {
      try {
        var state = getState();
        if (state) sessionStorage.setItem(KEY, JSON.stringify(state));
      } catch (e) {}
    }

    var sidebar = document.querySelector('.docs-sidebar');
    if (sidebar) {
      sidebar.addEventListener('click', function (e) {
        if (e.target.closest('summary')) {
          requestAnimationFrame(save);
        }
      });
    }

    document.addEventListener('visibilitychange', function () {
      if (document.visibilityState === 'hidden') save();
    });

    window.addEventListener('pagehide', save);
  } catch (e) {}
})();
