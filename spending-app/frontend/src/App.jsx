// App.jsx - Main component for the Spending App frontend
// This component renders the main page with Login and Sign Up buttons
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Landing, Login, SignUp, ProtectedPage } from './components';
import React from 'react';
import './styles/App.css';
import { AuthProvider } from './components/AuthContext';
import RequireAuth from './components/RequireAuth';

export default function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/" element={<Landing />} />
          <Route path="/login" element={<Login />} />
          <Route path="/signup" element={<SignUp />} />
          <Route path="/protected" element={
            <RequireAuth>
              <ProtectedPage />
            </RequireAuth>
          } />
        </Routes>
      </Router>
    </AuthProvider>
  );
}
