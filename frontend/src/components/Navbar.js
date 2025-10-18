import React, { useState, useEffect } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import axios from "axios";

function Navbar() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userName, setUserName] = useState("");
  const navigate = useNavigate();
  const location = useLocation();

  const checkAuthState = () => {
    const userId = localStorage.getItem("userId");
    const name = localStorage.getItem("name");
    
    if (userId && name) {
      setIsLoggedIn(true);
      setUserName(name);
    } else {
      setIsLoggedIn(false);
      setUserName("");
    }
  };

  useEffect(() => {
    // Check auth state on component mount
    checkAuthState();
  }, []);

  useEffect(() => {
    // Check auth state when route changes (e.g., after login)
    checkAuthState();
  }, [location.pathname]);

  useEffect(() => {
    // Listen for custom auth events
    const handleAuthChange = () => {
      checkAuthState();
    };

    window.addEventListener('authStateChanged', handleAuthChange);
    
    return () => {
      window.removeEventListener('authStateChanged', handleAuthChange);
    };
  }, []);

  const handleLogout = async () => {
    try {
      await axios.post("http://localhost:5097/api/auth/logout", {}, {
        withCredentials: true
      });
    } catch (err) {
      console.error("Logout error:", err);
    } finally {
      // Clear localStorage regardless of API response
      localStorage.removeItem("userId");
      localStorage.removeItem("name");
      localStorage.removeItem("email");
      setIsLoggedIn(false);
      setUserName("");
      
      // Dispatch custom event to update navbar
      window.dispatchEvent(new CustomEvent('authStateChanged'));
      
      navigate("/login");
    }
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container-fluid">
        <Link className="navbar-brand fw-bold" to="/dashboard">
          <span className="d-none d-sm-inline">Facesmash</span>
          <span className="d-sm-none">FS</span>
        </Link>
        
        <button 
          className="navbar-toggler" 
          type="button" 
          data-bs-toggle="collapse" 
          data-bs-target="#navbarNav"
          aria-controls="navbarNav" 
          aria-expanded="false" 
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>
        
        <div className="collapse navbar-collapse" id="navbarNav">
          {isLoggedIn ? (
            <>
              <ul className="navbar-nav me-auto">
                <li className="nav-item">
                  <Link className="nav-link" to="/dashboard">
                    <i className="fas fa-home d-none d-md-inline me-1"></i>
                    Dashboard
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/compare">
                    <i className="fas fa-balance-scale d-none d-md-inline me-1"></i>
                    Compare
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/profile">
                    <i className="fas fa-user d-none d-md-inline me-1"></i>
                    Profile
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/messages">
                    <i className="fas fa-envelope d-none d-md-inline me-1"></i>
                    Messages
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/leaderboard">
                    <i className="fas fa-trophy d-none d-md-inline me-1"></i>
                    Leaderboard
                  </Link>
                </li>
              </ul>
              <ul className="navbar-nav ms-auto">
                <li className="nav-item d-none d-md-block">
                  <span className="navbar-text me-3">
                    Welcome, {userName}!
                  </span>
                </li>
                <li className="nav-item d-md-none">
                  <span className="navbar-text me-3 small">
                    {userName}
                  </span>
                </li>
                <li className="nav-item">
                  <button 
                    className="btn btn-outline-light btn-sm responsive-btn" 
                    onClick={handleLogout}
                  >
                    <i className="fas fa-sign-out-alt d-none d-md-inline me-1"></i>
                    Logout
                  </button>
                </li>
              </ul>
            </>
          ) : (
            <ul className="navbar-nav ms-auto">
              <li className="nav-item">
                <Link className="nav-link" to="/login">
                  <i className="fas fa-sign-in-alt d-none d-md-inline me-1"></i>
                  Login
                </Link>
              </li>
              <li className="nav-item">
                <Link className="nav-link" to="/signup">
                  <i className="fas fa-user-plus d-none d-md-inline me-1"></i>
                  Sign Up
                </Link>
              </li>
            </ul>
          )}
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
