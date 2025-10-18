import React from "react";
import { Link, Outlet } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";

const Dashboard = () => {
  return (
    <div className="container-fluid responsive-spacing">
      <div className="row justify-content-center">
        <div className="col-12 col-lg-10 col-xl-8">
          <h1 className="responsive-title text-center text-center-mobile fade-in">
            Dating on Campus Reimagined
          </h1>

          {/* Responsive Navigation */}
          <nav className="nav responsive-nav mb-5 fade-in">
            <Link className="nav-link responsive-btn btn btn-outline-primary me-2 mb-2" to="/compare">
              <i className="fas fa-balance-scale d-none d-md-inline me-2"></i>
              Compare
            </Link>
            <Link className="nav-link responsive-btn btn btn-outline-primary me-2 mb-2" to="/profile">
              <i className="fas fa-user d-none d-md-inline me-2"></i>
              Profile
            </Link>
            <Link className="nav-link responsive-btn btn btn-outline-primary me-2 mb-2" to="/messages">
              <i className="fas fa-envelope d-none d-md-inline me-2"></i>
              Messages
            </Link>
            <Link className="nav-link responsive-btn btn btn-outline-primary me-2 mb-2" to="/leaderboard">
              <i className="fas fa-trophy d-none d-md-inline me-2"></i>
              Leaderboard
            </Link>
          </nav>

          {/* Page content */}
          <div className="content fade-in">
            <Outlet /> {/* Nested route content will render here */}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
