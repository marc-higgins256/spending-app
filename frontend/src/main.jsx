// main.jsx - Entry point for the React frontend

// Import StrictMode for highlighting potential problems in development
import { StrictMode } from 'react'
// Import createRoot to create a root React node for rendering
import { createRoot } from 'react-dom/client'
// Import global styles
import './styles/App.css';
// Import the main App component
import App from './App.jsx'

// Render the App component inside the element with id 'root'
// StrictMode is a tool for highlighting potential problems in an application
createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
