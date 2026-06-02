// Firestore CRUD + real-time listeners bridge.
import { db } from "./firebase-config.js";
import {
    collection, doc, addDoc, setDoc, updateDoc, deleteDoc,
    getDoc, getDocs, query, where, orderBy, limit, onSnapshot
} from "firebase/firestore";

const activeSubscriptions = new Map();

export async function addDocument(collectionPath, data) {
    const ref = await addDoc(collection(db, collectionPath), {
        ...data,
        createdAt: new Date().toISOString()
    });
    return ref.id;
}

export async function setDocument(documentPath, data) {
    await setDoc(doc(db, documentPath), data);
}

export async function updateDocument(documentPath, partialData) {
    await updateDoc(doc(db, documentPath), { ...partialData, updatedAt: new Date().toISOString() });
}

export async function deleteDocument(documentPath) {
    await deleteDoc(doc(db, documentPath));
}

export async function getDocument(documentPath) {
    const snap = await getDoc(doc(db, documentPath));
    return snap.exists() ? { id: snap.id, ...snap.data() } : null;
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
    const snap = await getDocs(q);
    return snap.docs.map(d => ({ id: d.id, ...d.data() }));
}

export function subscribeToDocument(documentPath, dotNetRef, methodName) {
    const subId = crypto.randomUUID();
    const unsub = onSnapshot(doc(db, documentPath), (snap) => {
        const data = snap.exists() ? { id: snap.id, ...snap.data() } : null;
        dotNetRef.invokeMethodAsync(methodName, data);
    });
    activeSubscriptions.set(subId, unsub);
    return subId;
}

export function subscribeToCollection(collectionPath, filters, dotNetRef, methodName) {
    const subId = crypto.randomUUID();
    const q = query(collection(db, collectionPath), ...buildConstraints(filters));
    const unsub = onSnapshot(q, (snap) => {
        const docs = snap.docs.map(d => ({ id: d.id, ...d.data() }));
        dotNetRef.invokeMethodAsync(methodName, docs);
    });
    activeSubscriptions.set(subId, unsub);
    return subId;
}

export function unsubscribe(subscriptionId) {
    const unsub = activeSubscriptions.get(subscriptionId);
    if (unsub) {
        unsub();
        activeSubscriptions.delete(subscriptionId);
    }
}
