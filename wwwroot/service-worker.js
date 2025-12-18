const CACHE_NAME = 'iinventory-cache-v2';
const urlsToCache = [
  '/',
  '/app.css',
  '/InventoryApp.styles.css',
  '/js/app.js',
  '/favicon.png',
  '_framework/blazor.web.js'
];

// CSS and JS files should always be fetched fresh
const networkFirstPatterns = [
  /\.css$/,
  /\.js$/
];

self.addEventListener('install', event => {
  // Skip waiting to activate immediately
  self.skipWaiting();
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

self.addEventListener('activate', event => {
  // Clean up old caches
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames
          .filter(name => name !== CACHE_NAME)
          .map(name => caches.delete(name))
      );
    })
  );
});

self.addEventListener('fetch', event => {
  const url = event.request.url;
  
  // For CSS and JS files, use network-first strategy
  const shouldNetworkFirst = networkFirstPatterns.some(pattern => pattern.test(url));
  
  if (shouldNetworkFirst) {
    event.respondWith(
      fetch(event.request)
        .then(response => {
          // Cache the fresh response
          const responseClone = response.clone();
          caches.open(CACHE_NAME).then(cache => {
            cache.put(event.request, responseClone);
          });
          return response;
        })
        .catch(() => {
          // Fallback to cache if network fails
          return caches.match(event.request);
        })
    );
  } else {
    // For other resources, use cache-first strategy
    event.respondWith(
      caches.match(event.request)
        .then(response => {
          if (response) {
            return response;
          }
          return fetch(event.request);
        })
    );
  }
});

