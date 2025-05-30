import React, { useEffect, useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";

const ConfirmEmail = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState("loading"); // loading, success, error
  const [error, setError] = useState("");

  useEffect(() => {
    const token = searchParams.get("token");
    if (!token) {
      setStatus("error");
      setError("No token provided.");
      return;
    }
    fetch(`/api/auth/confirm-email?token=${encodeURIComponent(token)}`)
      .then(async (res) => {
        if (res.ok) {
          setStatus("success");
          setTimeout(() => navigate("/email-verified"), 1500);
        } else {
          const data = await res.json().catch(() => ({}));
          setStatus("error");
          setError(data.message || "Invalid or expired token.");
        }
      })
      .catch(() => {
        setStatus("error");
        setError("Network error. Please try again later.");
      });
  }, [searchParams, navigate]);

  if (status === "loading") {
    return <div style={{ textAlign: "center", marginTop: 60 }}>Confirming your email...</div>;
  }
  if (status === "error") {
    return (
      <div style={{ textAlign: "center", marginTop: 60, color: "#d32f2f" }}>
        <h2>Email Confirmation Failed</h2>
        <p>{error}</p>
      </div>
    );
  }
  // status === "success" (briefly shown before redirect)
  return (
    <div style={{ textAlign: "center", marginTop: 60 }}>
      Email confirmed! Redirecting...
    </div>
  );
};

export default ConfirmEmail;
