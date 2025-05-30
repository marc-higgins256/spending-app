import React from "react";

export default function EmailPreview({ email, confirmationLink }) {
  return (
    <div style={{ maxWidth: 500, margin: "3rem auto", padding: 24, border: "1px solid #e0e0e0", borderRadius: 8, background: "#fafbfc" }}>
      <h2 style={{ color: "#1976d2" }}>Spending App Email Preview</h2>
      <p>To: <b>{email}</b></p>
      <p>Subject: <b>Confirm your email</b></p>
      <div style={{ margin: "2rem 0", padding: 16, background: "#fff", border: "1px solid #eee", borderRadius: 6 }}>
        <p>Click the link below to confirm your email address and activate your account:</p>
        <a href={confirmationLink} target="_blank" rel="noopener noreferrer" style={{ color: "#1976d2", wordBreak: "break-all" }}>
          {confirmationLink}
        </a>
      </div>
      <p style={{ color: "#888", fontSize: 13 }}>This is a preview for development/testing. No real email was sent.</p>
    </div>
  );
}
