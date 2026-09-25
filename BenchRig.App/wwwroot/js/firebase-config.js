// Firebase app initialization (ES module).
// Public-safe defaults are placeholders only. Replace these with your Firebase
// Web app config (identifiers, not server-side secrets) before enabling auth/data.
// Setup docs: FIREBASE_SETUP.md
import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";
import { getFirestore } from "firebase/firestore";

const defaults = {
    apiKey: "YOUR_FIREBASE_WEB_API_KEY",
    authDomain: "YOUR_PROJECT_ID.firebaseapp.com",
    projectId: "YOUR_PROJECT_ID",
    storageBucket: "YOUR_PROJECT_ID.firebasestorage.app",
    messagingSenderId: "YOUR_SENDER_ID",
    appId: "YOUR_APP_ID",
    measurementId: "YOUR_MEASUREMENT_ID"
};

const runtimeConfig =
    globalThis.__BENCHRIG_CONFIG__?.firebase ??
    globalThis.BenchRigConfig?.firebase ??
    {};

const firebaseConfig = {
    ...defaults,
    ...runtimeConfig
};

const isConfigured =
    firebaseConfig.apiKey &&
    firebaseConfig.projectId &&
    firebaseConfig.appId &&
    !firebaseConfig.apiKey.startsWith("YOUR_") &&
    !firebaseConfig.projectId.startsWith("YOUR_") &&
    !firebaseConfig.appId.startsWith("YOUR_");

if (!isConfigured) {
    console.warn("BenchRig Firebase config is using placeholders. Firebase-backed features remain disabled until configured. See FIREBASE_SETUP.md.");
}

export const app = initializeApp(firebaseConfig);
export const auth = getAuth(app);
export const db = getFirestore(app);
