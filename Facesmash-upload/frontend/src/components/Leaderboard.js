import React, { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { useUser } from "../contexts/UserContext";

function Leaderboard() {
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

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        const res = await axios.get("http://localhost:5097/api/Profile");
        setUsers(res.data);
      } catch (e) {
        console.error(e);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (!currentUser) return null; // Will redirect
  if (loading) return <div className="container mt-5">Loading...</div>;

  return (
    <div className="container mt-5">
      <h1 className="mb-4 text-center">Leaderboard 🏆</h1>
      <div className="list-group">
        {users.map((u) => (
          <div key={u.id} className="list-group-item d-flex justify-content-between align-items-center">
            <div>
              <span
                style={{ textDecoration: "underline", cursor: "pointer" }}
                onClick={() => navigate(`/messages/${u.id}`)}
              >
                {u.name}
              </span>
              <div className="text-muted small">Rating: {u.rating}</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Leaderboard;
