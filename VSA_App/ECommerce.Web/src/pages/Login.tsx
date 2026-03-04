import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import api from '../api';
import './Login.css';

export default function Login() {
    const [isRegister, setIsRegister] = useState(false);
    const [email, setEmail] = useState('demo@store.com');
    const [password, setPassword] = useState('Demo1234!');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [showPassword, setShowPassword] = useState(false);

    const login = useAuthStore((state) => state.login);
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            if (isRegister) {
                await api.post('/auth/register', { email, password, firstName, lastName });
                setIsRegister(false);
                setError('Registration successful! Please login.');
            } else {
                const { data } = await api.post('/auth/login', { email, password });
                login(data.user, data.token);
                navigate('/');
            }
        } catch (err: any) {
            setError(err.response?.data?.error || 'Authentication failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="login-container">
            <div className="login-split">
                <div className="login-brand">
                    <div className="brand-content">
                        <div className="logo-anim-wrapper">
                            <div className="logo-anim-border"></div>
                            <div className="logo-anim-inner"></div>
                            <img src="/logo.png" alt="SterlingPro" className="brand-logo-img" />
                        </div>
                        <p className="brand-tagline">Discover the Future of Shopping</p>
                    </div>
                    <div className="brand-glow"></div>
                </div>

                <div className="login-form-wrapper">
                    <div className="form-card">
                        <h2 className="form-title">{isRegister ? 'Join SterlingPro' : 'Enter the Store'}</h2>
                        {error && <div className={`toast ${error.includes('successful') ? 'toast-success' : 'toast-error'}`}>{error}</div>}

                        <form onSubmit={handleSubmit} className="auth-form">
                            {isRegister && (
                                <div className="form-row">
                                    <div className="form-group">
                                        <label>First Name</label>
                                        <input className="input-field" value={firstName} onChange={e => setFirstName(e.target.value)} required />
                                    </div>
                                    <div className="form-group">
                                        <label>Last Name</label>
                                        <input className="input-field" value={lastName} onChange={e => setLastName(e.target.value)} required />
                                    </div>
                                </div>
                            )}

                            <div className="form-group">
                                <label>Email</label>
                                <input className="input-field" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
                            </div>

                            <div className="form-group">
                                <label>Password</label>
                                <div className="password-wrapper">
                                    <input className="input-field" type={showPassword ? 'text' : 'password'} value={password} onChange={e => setPassword(e.target.value)} required />
                                    <button type="button" className="pw-toggle" onClick={() => setShowPassword(!showPassword)}>
                                        {showPassword ? 'Hide' : 'Show'}
                                    </button>
                                </div>
                            </div>

                            {!isRegister && (
                                <div className="form-options">
                                    <label className="checkbox"><input type="checkbox" /> Remember me</label>
                                    <a href="#" className="forgot-link">Forgot password?</a>
                                </div>
                            )}

                            <button type="submit" className="btn-primary auth-submit" disabled={loading}>
                                {loading ? <span className="spinner"></span> : (isRegister ? 'Register' : 'Enter')}
                            </button>
                        </form>

                        <p className="toggle-mode">
                            {isRegister ? 'Already have an account?' : 'New to SterlingPro?'}
                            <button className="toggle-btn" onClick={() => setIsRegister(!isRegister)}>
                                {isRegister ? 'Sign in' : 'Create an account'}
                            </button>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
