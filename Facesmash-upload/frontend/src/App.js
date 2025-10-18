import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";

import { UserProvider, useUser } from "./contexts/UserContext";
import Navbar from "./components/Navbar";
import Dashboard from "./components/Dashboard";
import Compare from "./components/Compare";
import Profile from "./components/Profile";
import Messages from "./components/Messages";
import Leaderboard from "./components/Leaderboard";
import Home from "./components/Home";
import Login from "./components/Login";

function AppRoutes() {
  const { currentUser } = useUser();

  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/login" element={<Login />} />
      <Route path="/profile" element={<Profile />} />
      <Route path="/dashboard" element={<Dashboard />} />
      <Route path="/compare" element={<Compare />} />
      <Route path="/messages" element={<Messages />} />
      <Route path="/messages/:userId" element={<Messages />} />
      <Route path="/leaderboard" element={<Leaderboard />} />
    </Routes>
  );
}

function App() {
  return (
    <UserProvider>
      <Router>
        <Navbar />
        <AppRoutes />
      </Router>
    </UserProvider>
  );
}

export default App;
