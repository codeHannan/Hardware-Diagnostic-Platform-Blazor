// Firestore CRUD + real-time listeners bridge.
import { db } from "./firebase-config.js";
import {
    collection, doc, addDoc, setDoc, updateDoc, deleteDoc,
    getDoc, getDocs, getCountFromServer, query, where, orderBy, limit, onSnapshot
} from "firebase/firestore";

const activeSubscriptions = new Map();
const TIMEOUT_MS = 12000;

// Firebase SDK operations have no built-in timeout, so an unreachable / unconfigured
// project would hang forever. Wrap each op so the UI can fail fast with a clear message.
function withTimeout(promise, label) {
    return Promise.race([
        promise,
        new Promise((_, reject) => setTimeout(
            () => reject(new Error(`FIRESTORE_UNREACHABLE: '${label}' timed out — check your Firebase config and that Firestore is enabled.`)),
            TIMEOUT_MS)),
    ]);
}

export async function addDocument(collectionPath, data) {
    const ref = await withTimeout(addDoc(collection(db, collectionPath), {
        ...data,
        createdAt: new Date().toISOString()
    }), "addDocument");
    return ref.id;
}

export async function setDocument(documentPath, data) {
    await withTimeout(setDoc(doc(db, documentPath), data), "setDocument");
}

export async function updateDocument(documentPath, partialData) {
    await withTimeout(updateDoc(doc(db, documentPath), { ...partialData, updatedAt: new Date().toISOString() }), "updateDocument");
}

export async function deleteDocument(documentPath) {
    await withTimeout(deleteDoc(doc(db, documentPath)), "deleteDocument");
}

export async function getDocument(documentPath) {
    const snap = await withTimeout(getDoc(doc(db, documentPath)), "getDocument");
    return snap.exists() ? { id: snap.id, ...snap.data() } : null;
}

// Document count of a (sub)collection — for accurate live comment/rating counts.
export async function countCollection(collectionPath) {
    try {
        const snap = await withTimeout(getCountFromServer(collection(db, collectionPath)), "countCollection");
        return snap.data().count;
    } catch { return 0; }
}

function buildConstraints(filters) {
    const constraints = [];
    if (filters) {
        if (filters.whereField && filters.whereOp && filters.whereValue !== undefined && filters.whereValue !== null) {
            constraints.push(where(filters.whereField, filters.whereOp, filters.whereValue));
        }
        if (filters.orderByField) {
            constraints.push(orderBy(filters.orderByField, filters.orderDir ?? "asc"));
        }
        if (filters.limitCount) {
            constraints.push(limit(filters.limitCount));
        }
    }
    return constraints;
}

export async function queryCollection(collectionPath, filters) {
    const q = query(collection(db, collectionPath), ...buildConstraints(filters));
    const snap = await withTimeout(getDocs(q), "queryCollection");
    return snap.docs.map(d => ({ id: d.id, ...d.data() }));
}

export function subscribeToDocument(documentPath, dotNetRef, methodName) {
    const subId = crypto.randomUUID();
    const unsub = onSnapshot(doc(db, documentPath),
        (snap) => { const data = snap.exists() ? { id: snap.id, ...snap.data() } : null; dotNetRef.invokeMethodAsync(methodName, data); },
        () => { /* listener error — ignore, the initial query already surfaced it */ });
    activeSubscriptions.set(subId, unsub);
    return subId;
}

export function subscribeToCollection(collectionPath, filters, dotNetRef, methodName) {
    const subId = crypto.randomUUID();
    const q = query(collection(db, collectionPath), ...buildConstraints(filters));
    const unsub = onSnapshot(q,
        (snap) => { const docs = snap.docs.map(d => ({ id: d.id, ...d.data() })); dotNetRef.invokeMethodAsync(methodName, docs); },
        () => { /* listener error — ignore */ });
    activeSubscriptions.set(subId, unsub);
    return subId;
}

export function unsubscribe(subscriptionId) {
    const unsub = activeSubscriptions.get(subscriptionId);
    if (unsub) { unsub(); activeSubscriptions.delete(subscriptionId); }
}
