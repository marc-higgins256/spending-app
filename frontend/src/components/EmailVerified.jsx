import React from "react";
import { Link } from "react-router-dom";

const EmailVerified = () => (
  <div style={{ maxWidth: 400, margin: "3rem auto", padding: 24, border: "1px solid #e0e0e0", borderRadius: 8, textAlign: "center" }}>
    <h2>Email Verified!</h2>
    <p>Your email has been successfully confirmed. You can now log in to your account.</p>
    <Link to="/login" style={{ display: "inline-block", marginTop: 16, padding: "8px 24px", background: "#1976d2", color: "#fff", borderRadius: 4, textDecoration: "none" }}>
      Go to Login
    </Link>
  </div>
);

export default EmailVerified;
