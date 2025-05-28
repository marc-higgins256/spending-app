import React from 'react';
import '../styles/Landing.css';

export default function Landing() {
  return (
    <div className="landing-center">
      <h1>Welcome to Spending App</h1>
      <div className="landing-links">
        <a href="/login">Login</a>
        <span>|</span>
        <a href="/signup">Sign Up</a>
      </div>
    </div>
  );
}
