import React, { useState } from "react";
import axios from "axios";
import { useNavigate, Link } from "react-router-dom";

function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage("");

    try {
      const res = await axios.post("http://localhost:5097/api/auth/login", {
        email,
        password,
      }, {
        withCredentials: true // Important for session cookies
      });
      
      setMessage(`Welcome ${res.data.name}!`);
      
      // Save minimal session info for immediate access
      localStorage.setItem("userId", String(res.data.userId));
      localStorage.setItem("name", res.data.name || "");
      localStorage.setItem("email", res.data.email || "");

      // Dispatch custom event to update navbar
      window.dispatchEvent(new CustomEvent('authStateChanged'));

      // Redirect to dashboard
      setTimeout(() => {
        navigate("/dashboard");
      }, 1000);
      
    } catch (err) {
      if (err.response?.status === 401) {
        setMessage("Invalid email or password");
      } else {
        setMessage("Login failed. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container-fluid responsive-spacing">
      <div className="row justify-content-center">
        <div className="col-12 col-sm-10 col-md-8 col-lg-6 col-xl-5">
          <div className="card responsive-card fade-in">
            <div className="card-body p-4">
              <div className="text-center mb-4">
                <h2 className="responsive-title mb-2">
                  <i className="fas fa-sign-in-alt me-2"></i>
                  Login
                </h2>
                <p className="responsive-text text-muted">Welcome back to Facesmash</p>
              </div>
              
              <form onSubmit={handleLogin} className="responsive-form">
                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    <i className="fas fa-envelope me-2"></i>
                    Email Address
                  </label>
                  <input
                    type="email"
                    className="form-control form-control-lg"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="Enter your email"
                    required
                  />
                </div>

                <div className="mb-4">
                  <label className="form-label fw-semibold">
                    <i className="fas fa-lock me-2"></i>
                    Password
                  </label>
                  <input
                    type="password"
                    className="form-control form-control-lg"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Enter your password"
                    required
                  />
                </div>

                <button 
                  type="submit" 
                  className="btn btn-primary w-100 responsive-btn"
                  disabled={isLoading}
                >
                  {isLoading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                      Logging in...
                    </>
                  ) : (
                    <>
                      <i className="fas fa-sign-in-alt me-2"></i>
                      Login
                    </>
                  )}
                </button>
              </form>

              {message && (
                <div className={`alert mt-4 ${message.includes("Welcome") ? "alert-success" : "alert-danger"} responsive-card`}>
                  <i className={`fas ${message.includes("Welcome") ? "fa-check-circle" : "fa-exclamation-triangle"} me-2`}></i>
                  {message}
                </div>
              )}

              <div className="text-center mt-4">
                <p className="responsive-text">
                  Don't have an account? 
                  <Link to="/signup" className="ms-2 fw-semibold text-decoration-none">
                    <i className="fas fa-user-plus me-1"></i>
                    Create one here
                  </Link>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
