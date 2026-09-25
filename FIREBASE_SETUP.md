# Firebase & Public Deployment Setup

This repository is prepared for **public source distribution**. It intentionally ships with placeholder Firebase values.

## 1) What is public vs secret

- **Safe in browser (identifiers):** Firebase Web config (`apiKey`, `authDomain`, `projectId`, `appId`, etc.).
- **Never commit / never ship to browser:** Firebase Admin SDK JSON, service-account private keys, API tokens, Stripe secret keys, Cloudflare API tokens, `.env` files.

## 2) Configure Firebase for this app

1. Create a Firebase project.
2. Enable **Authentication** (Email/Password; optional Google).
3. Enable **Cloud Firestore**.
4. In Firebase Console → Project settings → Your apps (Web), copy web config identifiers.
5. Set values in `BenchRig.App/wwwroot/js/firebase-config.js` (or inject runtime config through `window.__BENCHRIG_CONFIG__.firebase` before app bootstrap).
6. Deploy Firestore rules:

```bash
firebase deploy --only firestore:rules
```

## 3) GitHub Actions / Hosting setup

The workflow files use placeholders:

- `projectId: your-firebase-project-id`
- `secrets.FIREBASE_SERVICE_ACCOUNT`

Before enabling deployment workflows:

1. Replace placeholder `projectId` in workflow files.
2. Add repository secret `FIREBASE_SERVICE_ACCOUNT` with a deploy-capable service-account JSON.

## 4) Integration security boundaries

- **Firebase Auth + Firestore:** client-side SDK only; authorization enforced by `firestore.rules`.
- **Gemini (Firebase AI Logic):** configured through Firebase project backend; do not add direct provider secrets to browser code.
- **Stripe donations:** only publishable payment links in `stripe-interop.js`; do not commit secret keys (`sk_*`).
- **Cloudflare Worker:** deploy with your own account/tokens locally or CI secrets; do not commit credentials to repo.

## 5) Public release checklist

- [ ] No tracked `firebase-debug.log`, `.firebase/`, `release/`, `bin/`, `obj/`, `.env*`, or publish-profile secrets.
- [ ] No service-account JSON/private keys/tokens in tracked files.
- [ ] Firebase placeholders replaced with your project config identifiers (or Firebase-backed features intentionally left disabled).
- [ ] Firestore rules deployed and reviewed for least privilege.
- [ ] Repository builds locally (`dotnet build` / `dotnet publish`) with expected environment.
- [ ] Any previously exposed real credentials are revoked/rotated.
