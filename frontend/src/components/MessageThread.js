import React, { useState, useEffect } from "react";
import axios from "axios";

const MessageThread = ({ peerUser, onClose }) => {
  const [messages, setMessages] = useState([]);
  const [newMessage, setNewMessage] = useState("");
  const [loading, setLoading] = useState(true);
  const currentUserId = localStorage.getItem("userId");

  // Fetch messages between current user and peer
  const fetchMessages = async () => {
    try {
      const res = await axios.get(
        `http://localhost:5097/api/Messages/thread?userId=${currentUserId}&peerId=${peerUser.id}`
      );
      setMessages(res.data);
      setLoading(false);
    } catch (err) {
      console.error("Failed to fetch messages:", err);
      setLoading(false);
    }
  };

  // Send a new message
  const sendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim()) return;
    if (!currentUserId) {
      alert("Please log in to send messages");
      return;
    }

    try {
      const payload = {
        fromUserId: parseInt(currentUserId),
        toUserId: peerUser.id,
        content: newMessage.trim(),
      };

      console.log("Sending message payload:", payload);
      console.log("Current user ID:", currentUserId);
      console.log("Peer user ID:", peerUser.id);
      console.log("Peer user object:", peerUser);

      const response = await axios.post(
        "http://localhost:5097/api/Messages",
        payload,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      console.log("Message sent successfully:", response.data);
      setNewMessage("");
      fetchMessages(); // Refresh messages
    } catch (err) {
      console.error("Failed to send message:", err);
      console.error("Error response:", err.response);
      console.error("Error status:", err.response?.status);
      console.error("Error data:", err.response?.data);
      console.error("Error message:", err.message);
      alert(
        `Failed to send message: ${
          err.response?.data?.message || err.message || "Unknown error"
        }`
      );
    }
  };

  useEffect(() => {
    fetchMessages();
  }, [currentUserId, peerUser.id]);

  // Test backend connection
  const testConnection = async () => {
    try {
      const response = await axios.get("http://localhost:5097/api/leaderboard");
      console.log("Backend connection test successful:", response.status);
    } catch (err) {
      console.error("Backend connection test failed:", err);
    }
  };

  useEffect(() => {
    testConnection();
  }, []);

  if (loading)
    return <div className="text-center p-4">Loading messages...</div>;

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h4>Chat with {peerUser.name}</h4>
        <button className="btn btn-secondary" onClick={onClose}>
          Back to Compare
        </button>
      </div>

      <div className="card" style={{ height: "400px" }}>
        <div className="card-body d-flex flex-column">
          {/* Messages area */}
          <div
            className="flex-grow-1 overflow-auto mb-3"
            style={{ maxHeight: "300px" }}
          >
            {messages.length === 0 ? (
              <p className="text-muted text-center">
                No messages yet. Start the conversation!
              </p>
            ) : (
              messages.map((msg) => (
                <div
                  key={msg.id}
                  className={`mb-2 ${
                    msg.fromUserId === parseInt(currentUserId)
                      ? "text-end"
                      : "text-start"
                  }`}
                >
                  <div
                    className={`d-inline-block p-2 rounded ${
                      msg.fromUserId === parseInt(currentUserId)
                        ? "bg-primary text-white"
                        : "bg-light"
                    }`}
                    style={{ maxWidth: "70%" }}
                  >
                    <div>{msg.content}</div>
                    <small className="text-muted">
                      {new Date(msg.sentAt).toLocaleTimeString()}
                    </small>
                  </div>
                </div>
              ))
            )}
          </div>

          {/* Message input */}
          <form onSubmit={sendMessage} className="d-flex gap-2">
            <input
              type="text"
              className="form-control"
              placeholder="Type a message..."
              value={newMessage}
              onChange={(e) => setNewMessage(e.target.value)}
            />
            <button type="submit" className="btn btn-primary">
              Send
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default MessageThread;
