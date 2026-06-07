# Firebase setup (5 minutes, free)

The forum, knowledge base, support tickets, auth, and saved reports use Firebase
(Authentication + Firestore). The config shipped in the repo is a **placeholder** —
until you point it at your own free Firebase project you'll see *"Couldn't reach the
database"* and *"auth/configuration-not-found"*. Here's the whole setup.

## 1. Create a Firebase project
1. Go to **https://console.firebase.google.com** → **Add project**.
2. Name it (e.g. `benchrig`), accept defaults, finish. (Analytics optional.)

## 2. Register a Web app + copy the config
1. In the project, click the **`</>`** (Web) icon → register an app (any nickname).
2. Copy the `firebaseConfig` object it shows.
3. Paste it into **`BenchRig.App/wwwroot/js/firebase-config.js`**, replacing the
   placeholder object.

## 3. Enable Authentication
1. Left menu → **Build → Authentication → Get started**.
2. **Sign-in method** tab → enable **Email/Password**.
3. (Optional) enable **Google** for the "Continue with Google" button.
4. **Settings → Authorized domains**: `localhost` is already allowed. Add your
   deployed domain when you host it (e.g. `your-app.web.app`).

## 4. Create the Firestore database
1. Left menu → **Build → Firestore Database → Create database**.
2. Start in **production mode** (we deploy real rules next), pick a region, enable.

## 5. Deploy the security rules
The repo's `firestore.rules` (root) enforces the access model (public read of
posts/articles, authenticated writes, admin-only moderation, etc.).

**Easiest (console):** Firestore → **Rules** tab → paste the contents of
`firestore.rules` → **Publish**.

**Or with the CLI:**
```bash
npm i -g firebase-tools
firebase login
firebase init firestore   # select your project; point rules at ./firestore.rules
firebase deploy --only firestore:rules
```

## 6. Make yourself an admin (for the Admin pages)
1. Run the app, **Register** an account (creates `users/{uid}` with `role: "user"`).
2. In the Firestore console open that `users/{uid}` document and change
   **`role`** to **`admin`**.
3. Reload — the **Admin** section appears in the sidebar (tickets queue + KB editor).

## 7. Enable the AI chatbot — Firebase AI Logic
The assistant uses **Firebase AI Logic**, which calls Gemini through your own
Firebase project (no API key in the app, free Gemini Developer API tier):
1. Firebase console → **Build → AI Logic** → **Get started**.
2. Choose the **Gemini Developer API** provider (free; the **Vertex AI** provider needs Blaze).
3. (Recommended) enable **App Check** to protect your quota from abuse.
4. Reload the app and open the chat bubble — it works with no key.

This is the only chatbot backend; there is nothing else to configure.

## Done
Restart the app. Registration/login, the forum (post/comment/rate), the knowledge
base, and support tickets now read and write to your Firestore. If a call can't reach
the backend the UI fails fast (12s timeout) with a "Couldn't reach the database"
card and a retry button instead of hanging.

### Notes
- The web `apiKey` is **not a secret** — it only identifies the project. Access is
  controlled by Auth + the Firestore rules, which is why deploying the rules matters.
- Firestore free tier (Spark plan) is plenty for this app.
