// Stripe one-time donation via pre-created Payment Links (no backend).
// Replace the placeholder URLs with real links from the Stripe Dashboard.
const DONATION_LINKS = {
    500: "https://buy.stripe.com/test_YOUR_LINK_5",
    1000: "https://buy.stripe.com/test_YOUR_LINK_10",
    2500: "https://buy.stripe.com/test_YOUR_LINK_25",
    5000: "https://buy.stripe.com/test_YOUR_LINK_50",
    0: "https://buy.stripe.com/test_YOUR_LINK_CUSTOM"
};

export function redirectToCheckout(amountCents) {
    const link = DONATION_LINKS[amountCents] ?? DONATION_LINKS[0];
    window.open(link, "_blank", "noopener,noreferrer");
}
