// Firebase app initialization (ES module).
// ──────────────────────────────────────────────────────────────────────────
//  REPLACE the firebaseConfig values below with YOUR Firebase project's web config.
//  The values here are the implementation-plan placeholder and will NOT work
//  (auth/configuration-not-found + Firestore timeouts) until you point this at
//  a real project. See FIREBASE_SETUP.md in the repo root for the 5-minute setup
//  (create project → enable Email/Password auth → create Firestore → deploy rules).
//  Get your config: Firebase console → Project settings → "Your apps" → Web app.
//
//  NOTE: only edit the values inside firebaseConfig. The exports below (app/auth/db)
//  are required by the rest of the app — don't remove them.
// ──────────────────────────────────────────────────────────────────────────
import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";
import { getFirestore } from "firebase/firestore";

const firebaseConfig = {
    apiKey: "REPLACE_WITH_FIREBASE_API_KEY",
    authDomain: "REPLACE_WITH_FIREBASE_AUTH_DOMAIN",
    projectId: "REPLACE_WITH_FIREBASE_PROJECT_ID",
    storageBucket: "REPLACE_WITH_FIREBASE_STORAGE_BUCKET",
    messagingSenderId: "REPLACE_WITH_FIREBASE_MESSAGING_SENDER_ID",
    appId: "REPLACE_WITH_FIREBASE_APP_ID",
    measurementId: "REPLACE_WITH_FIREBASE_MEASUREMENT_ID"
};

export const app = initializeApp(firebaseConfig);
export const auth = getAuth(app);
export const db = getFirestore(app);
