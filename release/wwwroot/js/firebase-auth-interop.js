// Firebase Auth bridge functions.
import { auth } from "./firebase-config.js";
import {
    signInWithEmailAndPassword,
    createUserWithEmailAndPassword,
    signInWithPopup,
    GoogleAuthProvider,
    signOut,
    onAuthStateChanged,
    updateProfile,
    updatePassword,
    deleteUser,
    reauthenticateWithCredential,
    reauthenticateWithPopup,
    EmailAuthProvider
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

// ── Account management ──

// Change the signed-in user's display name (Firebase Auth profile).
export async function updateDisplayName(name) {
    const user = auth.currentUser;
    if (!user) throw new Error("Not signed in.");
    await updateProfile(user, { displayName: name });
    return mapUser(user);
}

// Change password (email/password accounts only). Re-authenticates first
// because Firebase requires a recent login for sensitive operations.
export async function changePassword(currentPassword, newPassword) {
    const user = auth.currentUser;
    if (!user) throw new Error("Not signed in.");
    const cred = EmailAuthProvider.credential(user.email, currentPassword);
    await reauthenticateWithCredential(user, cred);
    await updatePassword(user, newPassword);
}

// Permanently delete the account. Re-authenticates first: password accounts
// re-enter their password; federated (Google) accounts re-auth via popup.
export async function deleteAccount(currentPassword) {
    const user = auth.currentUser;
    if (!user) throw new Error("Not signed in.");
    if (passwordProvider(user)) {
        if (!currentPassword) throw new Error("Password required.");
        const cred = EmailAuthProvider.credential(user.email, currentPassword);
        await reauthenticateWithCredential(user, cred);
    } else {
        await reauthenticateWithPopup(user, googleProvider);
    }
    await deleteUser(user);
}

function passwordProvider(user) {
    return (user.providerData ?? []).some(p => p.providerId === "password");
}

function mapUser(user) {
    return {
        uid: user.uid,
        email: user.email,
        displayName: user.displayName ?? (user.email ? user.email.split("@")[0] : "User"),
        photoURL: user.photoURL,
        // First provider id ("password", "google.com", …) so the UI can decide
        // whether to offer the password-change form.
        providerId: user.providerData?.[0]?.providerId ?? "password"
    };
}
