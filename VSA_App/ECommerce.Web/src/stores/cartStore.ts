import { create } from 'zustand';
import api from '../api';

export interface CartItem {
    id: string; // product id basically based on backend return
    name: string;
    price: number;
    quantity: number;
    imageUrl: string;
}

interface CartState {
    items: CartItem[];
    totalItems: number;
    totalPrice: number;
    fetchCart: () => Promise<void>;
    addItem: (productId: string, quantity: number) => Promise<void>;
    updateQuantity: (productId: string, quantity: number) => Promise<void>;
    removeItem: (productId: string) => Promise<void>;
    clearCart: () => Promise<void>;
}

export const useCartStore = create<CartState>((set) => ({
    items: [],
    totalItems: 0,
    totalPrice: 0,
    fetchCart: async () => {
        try {
            const { data } = await api.get('/cart');
            set({ items: data.items, totalItems: data.totalItems, totalPrice: data.totalPrice });
        } catch {
            set({ items: [], totalItems: 0, totalPrice: 0 });
        }
    },
    addItem: async (productId, quantity) => {
        await api.post('/cart/items', { productId, quantity });
        await useCartStore.getState().fetchCart();
    },
    updateQuantity: async (productId, quantity) => {
        await api.put(`/cart/items/${productId}`, { quantity });
        await useCartStore.getState().fetchCart();
    },
    removeItem: async (productId) => {
        await api.delete(`/cart/items/${productId}`);
        await useCartStore.getState().fetchCart();
    },
    clearCart: async () => {
        await api.delete('/cart');
        await useCartStore.getState().fetchCart();
    },
}));
