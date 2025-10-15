import React, { useEffect, useState } from "react";
import axios from "axios";
import MessageThread from "./MessageThread";

const Compare = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedUser, setSelectedUser] = useState(null);

  // Fetch 2 random male users from backend
  const fetchUsers = async () => {
    try {
      setLoading(true);
      const res = await axios.get("http://localhost:5097/api/Compare/males"); // <-- FIXED
      setUsers(res.data);
      setLoading(false);
    } catch (err) {
      console.error(err);
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

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

  const openMessageThread = (user) => {
    setSelectedUser(user);
  };

  const closeMessageThread = () => {
    setSelectedUser(null);
  };

  if (loading || users.length < 2) return <p>Loading...</p>;

  if (selectedUser) {
    return (
      <MessageThread peerUser={selectedUser} onClose={closeMessageThread} />
    );
  }

  return (
    <div className="d-flex justify-content-center gap-5">
      {users.map((user) => (
        <div key={user.id} className="card" style={{ width: "200px" }}>
          <img src={user.photoUrl} className="card-img-top" alt={user.name} />
          <div className="card-body text-center">
            <h5 className="card-title">{user.name}</h5>
            <p className="card-text">{user.bio}</p>
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
  );
};

export default Compare;
