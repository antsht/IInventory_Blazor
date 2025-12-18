const CACHE_NAME = 'iinventory-cache-v1';
const urlsToCache = [
  '/',
  '/app.css',
  '/InventoryApp.styles.css',
  '/js/app.js',
  '/favicon.png',
  '_framework/blazor.web.js'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        if (response) {
          return response;
        }
        return fetch(event.request);
      })
  );
});

