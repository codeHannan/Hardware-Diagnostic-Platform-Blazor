// LeafletJS map for server geography. Leaflet (L) loaded via CDN in index.html.
let mapInstance = null;
let markerGroup = null;

export function initMap(elementId, lat, lng, zoom) {
    if (typeof L === "undefined") return;
    if (mapInstance) mapInstance.remove();

    mapInstance = L.map(elementId, { attributionControl: false }).setView([lat, lng], zoom);
    L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
        maxZoom: 18
    }).addTo(mapInstance);
    markerGroup = L.layerGroup().addTo(mapInstance);
}

export function plotServers(servers) {
    if (!mapInstance || !markerGroup) return;
    markerGroup.clearLayers();

    servers.forEach(s => {
        const color = s.pingMs == null ? "#948979"
            : s.pingMs < 50 ? "#6fbf73"
            : s.pingMs < 100 ? "#e0b341"
            : "#d9655b";
        const circle = L.circleMarker([s.lat, s.lng], {
            radius: 7, fillColor: color, color: "#DFD0B8", weight: 1.5, fillOpacity: 0.9
        });
        const label = s.pingMs != null
            ? `<b>${s.name}</b><br/>Ping: ${s.pingMs.toFixed(1)} ms`
            : `<b>${s.name}</b><br/>Ping: -`;
        circle.bindPopup(label);
        markerGroup.addLayer(circle);
    });

    if (servers.length > 0) {
        const bounds = L.latLngBounds(servers.map(s => [s.lat, s.lng]));
        mapInstance.fitBounds(bounds, { padding: [40, 40], maxZoom: 6 });
    }
}

export function plotUserLocation(lat, lng) {
    if (!mapInstance) return;
    L.circleMarker([lat, lng], {
        radius: 9, fillColor: "#6fa8c7", color: "#fff", weight: 2, fillOpacity: 1
    }).bindPopup("<b>Your Location</b>").addTo(mapInstance);
}

export function destroyMap() {
    if (mapInstance) {
        mapInstance.remove();
        mapInstance = null;
        markerGroup = null;
    }
}
