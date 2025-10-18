import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useUser } from "../contexts/UserContext";
import axios from "axios";

function Login() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const { selectUser } = useUser();
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    
    if (!email.trim()) {
      setMessage("Please enter an email address");
      return;
    }

    setLoading(true);
    setMessage("");

    try {
      // Get all users and find the one with matching email
      const res = await axios.get("http://localhost:5097/api/Profile");
      const users = res.data;
      const user = users.find(u => u.email.toLowerCase() === email.toLowerCase());
      
      if (user) {
        // User found, log them in
        selectUser(user);
        setMessage(`Welcome back, ${user.name}!`);
        setTimeout(() => {
          navigate("/dashboard");
        }, 1000);
      } else {
        // User not found
        setMessage("No user found with this email address");
      }
    } catch (error) {
      console.error("Login error:", error);
      setMessage("Error connecting to server. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleBackToHome = () => {
    navigate("/");
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-body">
              <h2 className="text-center mb-4">Login</h2>
              <p className="text-center text-muted mb-4">
                Enter your email address to access your account
              </p>
              
              <form onSubmit={handleLogin}>
                <div className="mb-3">
                  <label htmlFor="email" className="form-label">Email Address</label>
                  <input
                    type="email"
                    className="form-control"
                    id="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="Enter your email"
                    required
                  />
                </div>

                {message && (
                  <div className={`alert ${message.includes('Welcome') ? 'alert-success' : 'alert-danger'} mb-3`}>
                    {message}
                  </div>
                )}

                <div className="d-grid gap-2">
                  <button 
                    type="submit" 
                    className="btn btn-primary"
                    disabled={loading}
                  >
                    {loading ? "Logging in..." : "Login"}
                  </button>
                  <button 
                    type="button" 
                    className="btn btn-outline-secondary"
                    onClick={handleBackToHome}
                  >
                    Back to Home
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Login;
