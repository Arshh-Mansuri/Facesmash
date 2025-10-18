import React, { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import { handleImageError, getImageUrl } from "../utils/imageValidation";
import { useUser } from "../contexts/UserContext";

function Messages() {
  const { userId } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const { currentUser } = useUser();
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");
  const [conversations, setConversations] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState(userId ? parseInt(userId, 10) : null);
  const [loading, setLoading] = useState(false);

  // Use selected user ID instead of hardcoded value
  const currentUserId = currentUser?.id;
  const otherUserId = selectedUserId;
  const otherUserName = location.state?.userName || conversations.find(c => c.otherUser.id === otherUserId)?.otherUser.name;

  const canChat = useMemo(() => !!otherUserId && !!currentUserId, [otherUserId, currentUserId]);

  // Redirect to user selector if no user selected
  useEffect(() => {
    if (!currentUser) {
      navigate("/");
    }
  }, [currentUser, navigate]);

  // Load conversations
  useEffect(() => {
    if (!currentUserId) return; // Don't load if no user ID
    
    const loadConversations = async () => {
      try {
        setLoading(true);
        
        // Try to load from real API first
        try {
          const res = await axios.get(`http://localhost:5097/api/Message/conversations/${currentUserId}`);
          console.log("Real conversations loaded:", res.data);
          setConversations(res.data);
        } catch (apiError) {
          console.log("API not available, using mock data:", apiError.message);
          // Fallback to mock data if API is not available
          const mockConversations = [
            {
              otherUser: { id: 2, name: "Daisy", photoUrl: "https://randomuser.me/api/portraits/women/65.jpg" },
              lastMessage: { content: "Hi Jon! How are you?", sentAt: "2025-10-16T22:46:25.287359" },
              unreadCount: 0
            },
            {
              otherUser: { id: 3, name: "Ethan", photoUrl: "https://randomuser.me/api/portraits/men/52.jpg" },
              lastMessage: { content: "Test message from backend", sentAt: "2025-10-16T22:32:44.609963" },
              unreadCount: 0
            }
          ];
          setConversations(mockConversations);
        }
        
        // If no user selected, select the first conversation
        if (!selectedUserId && conversations.length > 0) {
          setSelectedUserId(conversations[0].otherUser.id);
        }
      } catch (e) {
        console.error("Error loading conversations:", e);
      } finally {
        setLoading(false);
      }
    };
    loadConversations();
  }, [currentUserId]);

  // Load messages for selected conversation
  useEffect(() => {
    if (!canChat) return;
    const fetchConversation = async () => {
      try {
        console.log(`Fetching messages between ${currentUserId} and ${otherUserId}`);
        const res = await axios.get(
          `http://localhost:5097/api/Message/between/${currentUserId}/${otherUserId}`
        );
        console.log("Messages received:", res.data);
        setMessages(res.data);
      } catch (e) {
        console.error("Error fetching messages:", e);
      }
    };
    fetchConversation();
  }, [canChat, currentUserId, otherUserId]);

  const sendMessage = async () => {
    if (!input.trim() || !canChat) return;
    try {
      console.log("Sending message:", { senderId: currentUserId, receiverId: otherUserId, content: input.trim() });
      const sendRes = await axios.post("http://localhost:5097/api/Message/send", {
        senderId: currentUserId,
        receiverId: otherUserId,
        content: input.trim(),
      });
      console.log("Message sent successfully:", sendRes.data);
      setInput("");
      const res = await axios.get(
        `http://localhost:5097/api/Message/between/${currentUserId}/${otherUserId}`
      );
      console.log("Refreshed messages:", res.data);
      setMessages(res.data);
    } catch (e) {
      console.error("Error sending message:", e);
    }
  };

  const selectConversation = (userId) => {
    setSelectedUserId(userId);
    navigate(`/messages/${userId}`, { replace: true });
  };

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now - date) / (1000 * 60 * 60);
    
    if (diffInHours < 24) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } else if (diffInHours < 168) { // 7 days
      return date.toLocaleDateString([], { weekday: 'short' });
    } else {
      return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
    }
  };

  if (loading) {
    return (
      <div className="container mt-5 text-center">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container-fluid mt-3">
      <div className="row" style={{ height: "80vh" }}>
        {/* Left Sidebar - Conversations List */}
        <div className="col-md-4 border-end">
          <div className="d-flex align-items-center p-3 border-bottom">
            <h4 className="m-0">Messages</h4>
          </div>
          
          <div className="conversations-list" style={{ height: "calc(100% - 80px)", overflowY: "auto" }}>
            {conversations.length === 0 ? (
              <div className="p-3 text-center text-muted">
                <p>No conversations yet</p>
                <small>Send messages to users from Compare or Leaderboard to start conversations!</small>
              </div>
            ) : (
              conversations.map((conversation) => (
                <div
                  key={conversation.otherUser.id}
                  className={`conversation-item p-3 border-bottom cursor-pointer ${
                    selectedUserId === conversation.otherUser.id ? 'bg-light' : ''
                  }`}
                  onClick={() => selectConversation(conversation.otherUser.id)}
                  style={{ cursor: 'pointer' }}
                >
                  <div className="d-flex align-items-center">
                    <img
                      src={getImageUrl(conversation.otherUser.photoUrl)}
                      alt={conversation.otherUser.name}
                      className="rounded-circle me-3"
                      style={{ width: "50px", height: "50px", objectFit: "cover" }}
                      onError={(e) => handleImageError(e, 50, 50)}
                    />
                    <div className="flex-grow-1">
                      <div className="d-flex justify-content-between align-items-start">
                        <h6 className="m-0 fw-bold">{conversation.otherUser.name}</h6>
                        {conversation.lastMessage && (
                          <small className="text-muted">{formatTime(conversation.lastMessage.sentAt)}</small>
                        )}
                      </div>
                      <p className="m-0 text-muted small" style={{ 
                        overflow: "hidden", 
                        textOverflow: "ellipsis", 
                        whiteSpace: "nowrap",
                        maxWidth: "200px"
                      }}>
                        {conversation.lastMessage ? conversation.lastMessage.content : "View conversation"}
                      </p>
                      {conversation.unreadCount > 0 && (
                        <span className="badge bg-primary rounded-pill">{conversation.unreadCount}</span>
                      )}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Right Side - Chat Messages */}
        <div className="col-md-8 d-flex flex-column">
          {!canChat ? (
            <div className="d-flex align-items-center justify-content-center h-100">
              <div className="text-center text-muted">
                <h3>Select a conversation</h3>
                <p>Choose a conversation from the left to start chatting.</p>
              </div>
            </div>
          ) : (
            <>
              {/* Chat Header */}
              <div className="d-flex align-items-center p-3 border-bottom bg-light">
                <img
                  src={getImageUrl(conversations.find(c => c.otherUser.id === otherUserId)?.otherUser.photoUrl)}
                  alt={otherUserName}
                  className="rounded-circle me-3"
                  style={{ width: "40px", height: "40px", objectFit: "cover" }}
                  onError={(e) => handleImageError(e, 40, 40)}
                />
                <div>
                  <h5 className="m-0">{otherUserName ?? `User ${otherUserId}`}</h5>
                  <small className="text-muted">Online</small>
                </div>
              </div>

              {/* Debug info - remove in production */}
              <div className="alert alert-info m-3">
                <small>
                  Debug: Current User ID: {currentUserId}, Other User ID: {otherUserId}, 
                  Messages Count: {messages.length}
                </small>
              </div>

              {/* Messages Area */}
              <div 
                className="flex-grow-1 p-3" 
                style={{ 
                  overflowY: "auto", 
                  backgroundColor: "#f8f9fa",
                  minHeight: "400px"
                }}
              >
                {messages.length === 0 ? (
                  <div className="text-center text-muted">
                    <p>No messages yet</p>
                    <small>Start the conversation!</small>
                  </div>
                ) : (
                  messages.map((m) => (
                    <div
                      key={m.id}
                      className={`d-flex ${m.senderId === currentUserId ? "justify-content-end" : "justify-content-start"} mb-3`}
                    >
                      <div
                        className={`px-3 py-2 rounded-3 ${m.senderId === currentUserId ? "bg-primary text-white" : "bg-white border"}`}
                        style={{ maxWidth: "70%" }}
                      >
                        <div>{m.content}</div>
                        <div className={`small ${m.senderId === currentUserId ? "text-white-50" : "text-muted"}`}>
                          {new Date(m.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </div>
                      </div>
                    </div>
                  ))
                )}
              </div>

              {/* Message Input */}
              <div className="p-3 border-top bg-white">
                <div className="d-flex gap-2">
                  <input
                    className="form-control"
                    placeholder="Type a message"
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && sendMessage()}
                  />
                  <button 
                    className="btn btn-primary" 
                    onClick={sendMessage}
                    disabled={!input.trim()}
                  >
                    <i className="bi bi-send"></i>
                  </button>
                </div>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default Messages;