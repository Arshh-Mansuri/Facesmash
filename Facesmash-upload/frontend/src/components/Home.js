import React from "react";
import { useNavigate } from "react-router-dom";

function Home() {
  const navigate = useNavigate();

  const handleLogin = () => {
    navigate("/login");
  };

  const handleCreateNewProfile = () => {
    navigate("/profile");
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-8">
          <div className="text-center mb-5">
            <h1 className="display-4 mb-3">Welcome to Facesmash</h1>
            <p className="lead text-muted">
              Compare profiles, send messages, and connect with others
            </p>
          </div>
          
          <div className="row justify-content-center">
            <div className="col-md-6 mb-4">
              <div className="card h-100 shadow-sm">
                <div className="card-body text-center">
                  <div className="mb-4">
                    <i className="fas fa-sign-in-alt fa-3x text-primary"></i>
                  </div>
                  <h4 className="card-title">Login</h4>
                  <p className="card-text">
                    Already have an account? Login with your email address to continue where you left off.
                  </p>
                  <button 
                    className="btn btn-primary btn-lg w-100"
                    onClick={handleLogin}
                  >
                    Login
                  </button>
                </div>
              </div>
            </div>
            
            <div className="col-md-6 mb-4">
              <div className="card h-100 shadow-sm">
                <div className="card-body text-center">
                  <div className="mb-4">
                    <i className="fas fa-user-plus fa-3x text-success"></i>
                  </div>
                  <h4 className="card-title">Create New Profile</h4>
                  <p className="card-text">
                    New to Facesmash? Create your profile to start comparing and messaging with other users.
                  </p>
                  <button 
                    className="btn btn-success btn-lg w-100"
                    onClick={handleCreateNewProfile}
                  >
                    Create New Profile
                  </button>
                </div>
              </div>
            </div>
          </div>
          
          <div className="text-center mt-5">
            <p className="text-muted small">
              Choose an option above to get started with Facesmash
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Home;
