import React, { useState, useEffect } from "react";
import axios from "axios";
import MessageThread from "./MessageThread";

function Messages() {
  const [chatThreads, setChatThreads] = useState([]);
  const [selectedThread, setSelectedThread] = useState(null);
  const [loading, setLoading] = useState(true);
  const currentUserId = localStorage.getItem("userId");

  // Fetch all users to show potential chat partners
  const fetchChatThreads = async () => {
    try {
      // For now, get all users except current user as potential chat partners
      const res = await axios.get("http://localhost:5097/api/leaderboard");
      const allUsers = res.data.filter(
        (user) => user.id !== parseInt(currentUserId)
      );
      setChatThreads(allUsers);
      setLoading(false);
    } catch (err) {
      console.error("Failed to fetch users:", err);
      setLoading(false);
    }
  };

  useEffect(() => {
    if (currentUserId) {
      fetchChatThreads();
    }
  }, [currentUserId]);

  const openThread = (user) => {
    setSelectedThread(user);
  };

  const closeThread = () => {
    setSelectedThread(null);
  };

  if (!currentUserId) {
    return (
      <div className="container text-center mt-5">
        <h1>Please log in to view messages</h1>
      </div>
    );
  }

  if (selectedThread) {
    return <MessageThread peerUser={selectedThread} onClose={closeThread} />;
  }

  if (loading) {
    return (
      <div className="container text-center mt-5">
        <h1>Loading messages...</h1>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <h2>Your Messages</h2>
      <p className="text-muted">
        Click on a user to start or continue a conversation
      </p>

      <div className="row">
        {chatThreads.map((user) => (
          <div key={user.id} className="col-md-6 col-lg-4 mb-3">
            <div
              className="card cursor-pointer"
              style={{ cursor: "pointer" }}
              onClick={() => openThread(user)}
            >
              <div className="card-body d-flex align-items-center">
                <img
                  src={user.photoUrl || "https://via.placeholder.com/50"}
                  alt={user.name}
                  className="rounded-circle me-3"
                  width="50"
                  height="50"
                />
                <div>
                  <h6 className="card-title mb-1">{user.name}</h6>
                  <small className="text-muted">{user.bio || "No bio"}</small>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {chatThreads.length === 0 && (
        <div className="text-center mt-5">
          <h4>No users to chat with yet</h4>
          <p>Go to the Compare page to find people to message!</p>
        </div>
      )}
    </div>
  );
}

export default Messages;
