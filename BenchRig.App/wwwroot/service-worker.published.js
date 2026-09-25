// PWA offline support was REMOVED — this is a self-destroying kill-switch worker.
// Clients that previously installed the old cache-first offline worker would otherwise
// keep serving stale content until a hard refresh. This version unregisters itself,
// purges all old caches, and reloads open tabs onto the fresh network build.
self.addEventListener('install', () => self.skipWaiting());

self.addEventListener('activate', event => {
    event.waitUntil((async () => {
        const keys = await caches.keys();
        await Promise.all(keys.map(key => caches.delete(key)));
        await self.registration.unregister();
        const clients = await self.clients.matchAll({ type: 'window' });
        clients.forEach(client => client.navigate(client.url));
    })());
});

// Always go to the network; never serve from the (now deleted) cache.
self.addEventListener('fetch', event => event.respondWith(fetch(event.request)));
