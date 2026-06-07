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
    apiKey: "AIzaSyBtVyQ1eZLPGp1nXA45s7mtc0VH8dDYRVs",
    authDomain: "diagnostic-platform-blazor.firebaseapp.com",
    projectId: "diagnostic-platform-blazor",
    storageBucket: "diagnostic-platform-blazor.firebasestorage.app",
    messagingSenderId: "876830402780",
    appId: "1:876830402780:web:1b15fbf39ee7d980cd2c19",
    measurementId: "G-NGN2CF3SJZ"
};

export const app = initializeApp(firebaseConfig);
export const auth = getAuth(app);
export const db = getFirestore(app);
