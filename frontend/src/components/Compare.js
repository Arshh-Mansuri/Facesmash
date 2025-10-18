import React, { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import MessageThread from "./MessageThread";

const Compare = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedUser, setSelectedUser] = useState(null);
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  // Fetch 2 random users from backend
  const fetchUsers = async () => {
    try {
      setLoading(true);
      setMessage(""); // Clear previous messages
      const res = await axios.get("http://localhost:5097/api/compare/random", {
        withCredentials: true, // Important for session cookies
      });
      setUsers(res.data);
      setLoading(false);
    } catch (err) {
      console.error("Error fetching users:", err);
      setLoading(false);
      if (err.response?.status === 401) {
        setMessage("Please log in to view comparisons.");
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else if (err.response?.status === 400) {
        setMessage("Not enough users available. Please create more accounts.");
      } else {
        setMessage(
          `Failed to load users: ${
            err.response?.data?.message || err.message || "Unknown error"
          }`
        );
      }
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []); // Fetch users on component mount

  const vote = async (winnerId, loserId) => {
    try {
      await axios.post(
        "http://localhost:5097/api/compare/vote",
        {
          winnerId,
          loserId,
        },
        {
          withCredentials: true, // Important for session cookies
        }
      );
      fetchUsers(); // Fetch new users after vote
    } catch (err) {
      if (err.response?.status === 401) {
        setMessage("Please log in to vote.");
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else {
        console.error(err);
        setMessage("Failed to submit vote. Please try again.");
      }
    }
  };

  const openMessageThread = (user) => {
    setSelectedUser(user);
  };

  const closeMessageThread = () => {
    setSelectedUser(null);
  };

  if (loading) {
    return (
      <div className="text-center">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
        <p className="mt-2">Loading users...</p>
        {message && (
          <div
            className={`alert ${
              message.includes("log in") ? "alert-warning" : "alert-danger"
            }`}
          >
            {message}
          </div>
        )}
      </div>
    );
  }

  if (users.length < 2) {
    return (
      <div className="text-center">
        <div className="alert alert-warning">
          <h5>Not enough users</h5>
          <p>We need at least 2 users to compare.</p>
          <button className="btn btn-primary" onClick={fetchUsers}>
            Try Again
          </button>
        </div>
        {message && (
          <div
            className={`alert ${
              message.includes("log in") ? "alert-warning" : "alert-danger"
            }`}
          >
            {message}
          </div>
        )}
      </div>
    );
  }

  if (selectedUser) {
    return (
      <MessageThread peerUser={selectedUser} onClose={closeMessageThread} />
    );
  }

  return (
    <div>
      {message && (
        <div
          className={`alert ${
            message.includes("log in") ? "alert-warning" : "alert-danger"
          } mb-3`}
        >
          {message}
        </div>
      )}

      <div className="d-flex justify-content-center gap-5">
        {users.map((user) => (
          <div key={user.id} className="card" style={{ width: "200px" }}>
            <div className="position-relative">
              <img
                src={user.photoUrl}
                className="card-img-top"
                alt={user.name}
              />
              {user.photoUrl && user.photoUrl.includes("placeholder") && (
                <div className="position-absolute top-0 end-0 m-2">
                  <span className="badge bg-warning text-dark">No Photo</span>
                </div>
              )}
            </div>
            <div className="card-body text-center">
              <h5 className="card-title">{user.name}</h5>
              <p className="card-text">{user.bio || "No bio yet"}</p>
              <p className="badge bg-primary">Rating: {user.rating}</p>
              <div className="d-flex gap-2 justify-content-center mt-2">
                <button
                  className="btn btn-primary btn-sm"
                  onClick={() =>
                    vote(user.id, users.find((u) => u.id !== user.id).id)
                  }
                >
                  Vote
                </button>
                <button
                  className="btn btn-success btn-sm"
                  onClick={() => openMessageThread(user)}
                >
                  Message
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Compare;
