import { useEffect, useState } from 'react';
import { useAuthStore } from '../stores/authStore';
import { useCartStore } from '../stores/cartStore';
import api from '../api';
import { Search, ShoppingBag, User as UserIcon, LogOut, Plus, Minus, Trash2, X, Check } from 'lucide-react';
import './Store.css';

export default function Store() {
    const { user, logout } = useAuthStore();
    const { items, totalItems, totalPrice, fetchCart, addItem, updateQuantity, removeItem, clearCart } = useCartStore();

    const [products, setProducts] = useState<any[]>([]);
    const [categories, setCategories] = useState<any[]>([{ id: null, name: 'All' }]);
    const [activeCategory, setActiveCategory] = useState<number | null>(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [sortBy, setSortBy] = useState('price');
    const [sortOrder, setSortOrder] = useState('asc');
    const [isCartOpen, setIsCartOpen] = useState(false);
    const [showDropdown, setShowDropdown] = useState(false);
    const [addedItems, setAddedItems] = useState<Record<string, boolean>>({});

    useEffect(() => {
        fetchCart();
        // For demo purposes hardcoded categories due to missing get endpoints in the backend (using what was specified)
        setCategories([
            { id: null, name: 'All' },
            { id: 1, name: 'Electronics' },
            { id: 2, name: 'Clothing' },
            { id: 3, name: 'Books' },
            { id: 4, name: 'Home & Garden' },
            { id: 5, name: 'Sports' }
        ]);
    }, []);

    useEffect(() => {
        const delayDebounceRequest = setTimeout(() => {
            loadProducts();
        }, 300);
        return () => clearTimeout(delayDebounceRequest);
    }, [searchTerm, activeCategory, sortBy, sortOrder]);

    const loadProducts = async () => {
        try {
            const params = new URLSearchParams({
                page: '1',
                pageSize: '12',
                sortBy,
                sortOrder,
            });
            if (activeCategory) params.append('categoryId', activeCategory.toString());
            if (searchTerm) params.append('search', searchTerm);

            const { data } = await api.get(`/products?${params.toString()}`);
            setProducts(data.data);
        } catch (e) {
            console.error(e);
        }
    };

    const handleAddToCart = async (product: any) => {
        setAddedItems(prev => ({ ...prev, [product.id]: true }));
        await addItem(product.id, 1);
        setTimeout(() => {
            setAddedItems(prev => ({ ...prev, [product.id]: false }));
        }, 1500);
    };

    const handleCheckout = () => {
        clearCart();
        alert('Order placed successfully!');
        setIsCartOpen(false);
    };

    return (
        <div className="store-layout">
            {/* Navigation */}
            <nav className="navbar">
                <div className="nav-container">
                    <div className="logo font-display">
                        <img src="/logo.png" alt="SterlingPro" className="nav-logo-img" />
                    </div>

                    <div className="search-bar">
                        <Search className="search-icon" size={18} />
                        <input
                            type="text"
                            placeholder="Search products..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>

                    <div className="nav-actions">
                        <div className="user-menu" onClick={() => setShowDropdown(!showDropdown)}>
                            <div className="avatar">
                                <UserIcon size={18} />
                            </div>
                            <span className="user-name">{user?.firstName}</span>
                            {showDropdown && (
                                <div className="dropdown">
                                    <button onClick={logout}><LogOut size={16} /> Logout</button>
                                </div>
                            )}
                        </div>

                        <button className="cart-btn" onClick={() => setIsCartOpen(true)}>
                            <ShoppingBag size={22} />
                            {totalItems > 0 && <span className="cart-badge">{totalItems}</span>}
                        </button>
                    </div>
                </div>
            </nav>

            {/* Hero Section */}
            <header className="hero">
                <div className="hero-content">
                    <h1>Discover the Future of Shopping</h1>
                    <p>Curated premium tech & gear for the modern visionary.</p>
                    <button className="btn-primary" onClick={() => window.scrollTo({ top: 500, behavior: 'smooth' })}>Shop Now</button>
                </div>
                <div className="hero-glow"></div>
            </header>

            <main className="container main-content">
                <div className="store-controls">
                    {/* Categories */}
                    <div className="categories-list">
                        {categories.map(c => (
                            <button
                                key={c.id || 'all'}
                                className={`category-pill ${activeCategory === c.id ? 'active' : ''}`}
                                onClick={() => setActiveCategory(c.id)}
                            >
                                {c.name}
                            </button>
                        ))}
                    </div>

                    {/* Sort */}
                    <select
                        className="sort-select"
                        onChange={(e) => {
                            const [by, order] = e.target.value.split('-');
                            setSortBy(by);
                            setSortOrder(order);
                        }}
                        value={`${sortBy}-${sortOrder}`}
                    >
                        <option value="price-asc">Price: Low to High</option>
                        <option value="price-desc">Price: High to Low</option>
                        <option value="name-asc">Name: A to Z</option>
                        <option value="newest-desc">Newest Arrivals</option>
                    </select>
                </div>

                {/* Product Grid */}
                <div className="product-grid">
                    {products.map(p => (
                        <div className="product-card" key={p.id}>
                            <div className="product-img-wrapper">
                                <div className="category-badge">{p.category.name}</div>
                                <img src={p.imageUrl} alt={p.name} loading="lazy" />
                            </div>
                            <div className="product-info">
                                <h3 className="product-name">{p.name}</h3>
                                <p className="product-desc">{p.description}</p>

                                <div className="product-meta">
                                    <span className="product-price mono">₦{p.price.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span>
                                    <span className={`stock-status ${p.stockQuantity < 5 ? 'low' : ''}`}>
                                        <span className="dot"></span> {p.stockQuantity < 5 ? 'Low Stock' : 'In Stock'}
                                    </span>
                                </div>

                                {addedItems[p.id] ? (
                                    <button className="add-to-cart success" disabled>
                                        <Check size={18} /> Added
                                    </button>
                                ) : (
                                    <button className="add-to-cart" onClick={() => handleAddToCart(p)}>
                                        Add to Cart
                                    </button>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            </main>

            {/* Cart Drawer Overlay */}
            <div className={`cart-overlay ${isCartOpen ? 'open' : ''}`} onClick={() => setIsCartOpen(false)}></div>
            <aside className={`cart-drawer ${isCartOpen ? 'open' : ''}`}>
                <div className="cart-header">
                    <h2>Your Cart ({totalItems})</h2>
                    <button className="close-cart" onClick={() => setIsCartOpen(false)}><X size={24} /></button>
                </div>

                <div className="cart-items">
                    {items.length === 0 ? (
                        <div className="empty-cart">
                            <ShoppingBag size={48} />
                            <p>Your cart is empty.</p>
                        </div>
                    ) : (
                        items.map((item) => (
                            <div className="cart-item" key={item.id}>
                                <img src={item.imageUrl} alt={item.name} />
                                <div className="cart-item-info">
                                    <h4>{item.name}</h4>
                                    <div className="cart-item-meta">
                                        <span className="mono text-accent">₦{item.price.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span>
                                        <div className="quantity-controls">
                                            <button onClick={() => updateQuantity(item.id, item.quantity - 1)}><Minus size={14} /></button>
                                            <span className="mono">{item.quantity}</span>
                                            <button onClick={() => updateQuantity(item.id, item.quantity + 1)}><Plus size={14} /></button>
                                        </div>
                                    </div>
                                </div>
                                <button className="remove-item" onClick={() => removeItem(item.id)}><Trash2 size={18} /></button>
                            </div>
                        ))
                    )}
                </div>

                <div className="cart-footer">
                    <div className="cart-total">
                        <span>Subtotal</span>
                        <span className="mono">₦{totalPrice.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span>
                    </div>
                    <button className="btn-primary checkout-btn" disabled={items.length === 0} onClick={handleCheckout}>
                        Checkout
                    </button>
                    <button className="continue-btn" onClick={() => setIsCartOpen(false)}>Continue Shopping</button>
                </div>
            </aside>
        </div>
    );
}
