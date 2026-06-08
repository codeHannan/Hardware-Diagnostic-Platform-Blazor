// Firebase Auth bridge functions.
import { auth } from "./firebase-config.js";
import {
    signInWithEmailAndPassword,
    createUserWithEmailAndPassword,
    signInWithPopup,
    GoogleAuthProvider,
    signOut,
    onAuthStateChanged,
    updateProfile
} from "firebase/auth";

const googleProvider = new GoogleAuthProvider();

export async function signInWithEmail(email, password) {
    const cred = await signInWithEmailAndPassword(auth, email, password);
    return mapUser(cred.user);
}

export async function registerWithEmail(email, password, displayName) {
    const cred = await createUserWithEmailAndPassword(auth, email, password);
    if (displayName) await updateProfile(cred.user, { displayName });
    return mapUser(cred.user);
}

export async function signInWithGoogle() {
    const cred = await signInWithPopup(auth, googleProvider);
    return mapUser(cred.user);
}

export async function firebaseSignOut() {
    await signOut(auth);
}

export function registerAuthStateListener(dotNetRef, methodName) {
    return onAuthStateChanged(auth, (user) => {
        const mapped = user ? mapUser(user) : null;
        dotNetRef.invokeMethodAsync(methodName, mapped);
    });
}

function mapUser(user) {
    return {
        uid: user.uid,
        email: user.email,
        displayName: user.displayName ?? (user.email ? user.email.split("@")[0] : "User"),
        photoURL: user.photoURL
    };
}
