import React, { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { handleImageError, getImageUrl } from "../utils/imageValidation";
import { useUser } from "../contexts/UserContext";

const Compare = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const { currentUser } = useUser();
  const navigate = useNavigate();

  // Redirect to home if no user selected
  useEffect(() => {
    if (!currentUser) {
      navigate("/");
    }
  }, [currentUser, navigate]);

  // Fetch 2 random male users from backend (excluding current user)
  const fetchUsers = async () => {
    try {
      setLoading(true);
      const url = currentUser 
        ? `http://localhost:5097/api/Compare/males?excludeUserId=${currentUser.id}`
        : "http://localhost:5097/api/Compare/males";
      const res = await axios.get(url);
      setUsers(res.data);
      setLoading(false);
    } catch (err) {
      console.error(err);
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [currentUser]);

  const vote = async (winnerId, loserId) => {
    try {
      await axios.post("http://localhost:5097/api/compare/vote", {
        winnerId,
        loserId,
      });
      fetchUsers(); // Fetch new users after vote
    } catch (err) {
      console.error(err);
    }
  };

  if (!currentUser) return null; // Will redirect
  if (loading || users.length < 2) return <p>Loading...</p>;

  return (
    <div className="d-flex justify-content-center gap-5">
      {users.map((user) => (
        <div
          key={user.id}
          className="card"
          style={{ width: "200px", cursor: "pointer" }}
          onClick={() => vote(user.id, users.find((u) => u.id !== user.id).id)}
        >
          <img 
            src={getImageUrl(user.photoUrl)} 
            className="card-img-top" 
            alt={user.name}
            style={{ height: "200px", objectFit: "cover" }}
            onError={(e) => handleImageError(e, 200, 200)}
          />
          <div className="card-body text-center">
            <h5
              className="card-title"
              style={{ textDecoration: "underline", cursor: "pointer" }}
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/messages/${user.id}`);
              }}
            >
              {user.name}
            </h5>
            <p className="card-text">{user.bio}</p>
          </div>
        </div>
      ))}
    </div>
  );
};

export default Compare;
