import React, { useState } from 'react';
import '../styles/SignUp.css';

// SignUp component handles user registration form and submission
export default function SignUp() {
  // State for form fields
  const [form, setForm] = useState({ username: '', email: '', password: '', confirmPassword: '' });
  // State for feedback message to the user
  const [message, setMessage] = useState('');
  // State to indicate loading/submitting status
  const [loading, setLoading] = useState(false);

  // Destructure form fields for easy access
  const { username, email, password, confirmPassword } = form;

  // Handle input changes and update form state
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage('');
    // Basic frontend validation
    if (!username || !email || !password || !confirmPassword) {
      setMessage('All fields are required.');
      return;
    }
    if (password !== confirmPassword) {
      setMessage('Passwords do not match.');
      return;
    }
    setLoading(true);
    try {
      // Send POST request to backend API
      const res = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, email, password })
      });
      const data = await res.json();
      if (res.ok) {
        // Success: clear form and show message
        setMessage('Sign up successful!');
        setForm({ username: '', email: '', password: '', confirmPassword: '' });
        // Store JWT if returned (optional, for auto-login)
        if (data.token) {
          localStorage.setItem('token', data.token);
        }
      } else {
        // Show error message from backend or fallback
        setMessage(data.message || data.error || 'Sign up failed.');
      }
    } catch {
      // Network or server error
      setMessage('Error connecting to server.');
    }
    setLoading(false);
  };

  // Render the sign up form
  return (
    <div className="signup-container">
      <h2>Sign Up</h2>
      <form onSubmit={handleSubmit} className="signup-form">
        <input
          type="text"
          name="username"
          placeholder="Username"
          value={username}
          onChange={handleChange}
          required
        />
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={email}
          onChange={handleChange}
          required
        />
        <input
          type="password"
          name="password"
          placeholder="Password"
          value={password}
          onChange={handleChange}
          required
        />
        <input
          type="password"
          name="confirmPassword"
          placeholder="Confirm Password"
          value={confirmPassword}
          onChange={handleChange}
          required
        />
        <button type="submit" disabled={loading}>
          {loading ? 'Signing up...' : 'Sign Up'}
        </button>
      </form>
      {/* Show feedback message if present */}
      {message && <div className="signup-message">{message}</div>}
    </div>
  );
}