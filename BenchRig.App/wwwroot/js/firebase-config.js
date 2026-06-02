// Firebase app initialization (ES module).
// Project: diagnostic-platform-blazor
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

const app = initializeApp(firebaseConfig);

export const auth = getAuth(app);
export const db = getFirestore(app);
