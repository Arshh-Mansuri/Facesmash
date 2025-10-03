import React from "react";
import { Link, Outlet } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";

const Dashboard = () => {
  return (
    <div className="container mt-4">
      <h1 className="text-center mb-4">Dating on Campus Reimagined</h1>

      {/* Navigation */}
      <nav className="nav justify-content-center mb-5">
        <Link className="nav-link" to="/compare">
          Compare
        </Link>
        <Link className="nav-link" to="/profile">
          Profile
        </Link>
        <Link className="nav-link" to="/messages">
          Messages
        </Link>
        <Link className="nav-link" to="/leaderboard">
          Leaderboard
        </Link>
      </nav>

      {/* Page content */}
      <div className="content">
        <Outlet /> {/* Nested route content will render here */}
      </div>
    </div>
  );
};

export default Dashboard;
