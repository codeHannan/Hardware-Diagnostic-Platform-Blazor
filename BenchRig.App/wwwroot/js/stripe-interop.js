// Stripe one-time / recurring donations via pre-created Payment Links (no backend, no API keys).
// ──────────────────────────────────────────────────────────────────────────
//  SETUP: In the Stripe Dashboard → Payment links, create a link per amount
//  (https://dashboard.stripe.com/payment-links), then paste each URL below.
//  Until the "YOUR_LINK" placeholders are replaced, the Donate page shows a
//  friendly "not set up yet" card instead of opening a broken tab.
//   • oneTime  — one-off donation links (required)
//   • monthly  — optional recurring (subscription) links; leave as placeholders to hide the Monthly toggle
//   • custom   — optional link configured to let the donor enter their own amount
// ──────────────────────────────────────────────────────────────────────────
const CONFIG = {
    currency: "usd",
    oneTime: [
        { cents: 500,  label: "Coffee",    url: "https://buy.stripe.com/test_YOUR_LINK_5" },
        { cents: 1000, label: "Supporter", url: "https://buy.stripe.com/test_YOUR_LINK_10" },
        { cents: 2500, label: "Backer",    url: "https://buy.stripe.com/test_YOUR_LINK_25" },
        { cents: 5000, label: "Sponsor",   url: "https://buy.stripe.com/test_YOUR_LINK_50" },
    ],
    monthly: [
        { cents: 500,  label: "Coffee",    url: "https://buy.stripe.com/test_YOUR_MONTHLY_5" },
        { cents: 1000, label: "Supporter", url: "https://buy.stripe.com/test_YOUR_MONTHLY_10" },
        { cents: 2500, label: "Backer",    url: "https://buy.stripe.com/test_YOUR_MONTHLY_25" },
    ],
    custom: { url: "https://buy.stripe.com/test_YOUR_LINK_CUSTOM" },
};

const PLACEHOLDER = /YOUR_(LINK|MONTHLY)/i;
function isReal(url) {
    return typeof url === "string" && /^https:\/\/(buy\.stripe\.com|.*\.stripe\.com)\//i.test(url) && !PLACEHOLDER.test(url);
}

function mapTiers(arr) {
    return (arr || []).map(t => ({
        cents: t.cents | 0,
        label: t.label || "",
        url: t.url || "",
        configured: isReal(t.url),
    }));
}

export function getDonationOptions() {
    const oneTime = mapTiers(CONFIG.oneTime);
    const monthly = mapTiers(CONFIG.monthly);
    const custom = CONFIG.custom && isReal(CONFIG.custom.url)
        ? { cents: 0, label: "Custom", url: CONFIG.custom.url, configured: true }
        : null;

    const oneTimeConfigured = oneTime.some(t => t.configured);
    const monthlyConfigured = monthly.some(t => t.configured);

    return {
        currency: CONFIG.currency || "usd",
        oneTime,
        monthly,
        custom,
        oneTimeConfigured,
        monthlyConfigured,
        configured: oneTimeConfigured || monthlyConfigured || !!(custom && custom.configured),
    };
}

// Opens a real Stripe link in a new tab; refuses placeholders/invalid URLs (returns false).
export function openDonation(url) {
    if (!isReal(url)) return false;
    window.open(url, "_blank", "noopener,noreferrer");
    return true;
}
