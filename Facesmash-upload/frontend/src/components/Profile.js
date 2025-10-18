import React, { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { validateImageFile, handleImageError, getImageUrl } from "../utils/imageValidation";
import { useUser } from "../contexts/UserContext";

function Profile() {
  const navigate = useNavigate();
  const { currentUser, selectUser } = useUser();
  const [user, setUser] = useState(null);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [bio, setBio] = useState("");
  const [gender, setGender] = useState("");
  const [photo, setPhoto] = useState(null);
  const [saving, setSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState("");

  // Check if this is a new user registration or existing user edit
  const isNewUser = !currentUser;
  const userId = currentUser?.id;

  useEffect(() => {
    // Only load existing user data if we have a current user
    if (!isNewUser && userId) {
      const load = async () => {
        try {
          const res = await axios.get(`http://localhost:5097/api/Profile/${userId}`);
          setUser(res.data);
          setName(res.data.name ?? "");
          setEmail(res.data.email ?? "");
          setBio(res.data.bio ?? "");
          setGender(res.data.gender ?? "");
        } catch (e) {
          console.error(e);
        }
      };
      load();
    }
  }, [userId, isNewUser]);

  const onSubmit = async (e) => {
    e.preventDefault();
    
    // Basic validation
    if (!name.trim() || !email.trim()) {
      alert("Name and email are required");
      return;
    }
    
    setSaving(true);
    try {
      // Validate image format
      if (photo) {
        const validation = validateImageFile(photo);
        if (!validation.isValid) {
          alert(validation.error);
          setSaving(false);
          return;
        }
      }

      const form = new FormData();
      form.append("name", name);
      form.append("email", email);
      form.append("bio", bio);
      form.append("gender", gender);
      if (photo) form.append("photo", photo);

      let res;
      if (isNewUser) {
        // Create new user
        console.log("Creating new user with data:", { name, email, bio, gender, photo: photo?.name });
        res = await axios.post(`http://localhost:5097/api/Profile/create`, form, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        console.log("User created successfully:", res.data);
        // Set the new user as current user
        selectUser(res.data);
        setSaveMessage("Profile created successfully! Welcome to Facesmash!");
      } else {
        // Update existing user
        console.log("Updating existing user:", userId);
        res = await axios.post(`http://localhost:5097/api/Profile/${userId}/update`, form, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        console.log("User updated successfully:", res.data);
        setSaveMessage("Profile updated successfully!");
      }
      
      setUser(res.data);
      console.log("Profile saved:", res.data);
      
      // Clear the photo input after successful upload
      setPhoto(null);
      // Reset the file input
      const fileInput = document.querySelector('input[type="file"]');
      if (fileInput) fileInput.value = '';
      
      // Navigate to compare page after a delay
      setTimeout(() => {
        setSaveMessage("");
        navigate("/compare");
      }, 2000);
    } catch (e) {
      console.error("Profile save error:", e);
      console.error("Error response:", e.response);
      if (e.response?.data) {
        alert(`Error: ${e.response.data}`);
      } else if (e.message) {
        alert(`Error: ${e.message}`);
      } else {
        alert("Error saving profile. Please try again.");
      }
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="container mt-5" style={{ maxWidth: 700 }}>
      <h1 className="mb-4 text-center">
        {isNewUser ? "Create Your Profile" : "Edit Profile"}
      </h1>
      {isNewUser && (
        <div className="alert alert-info mb-4">
          <strong>Welcome to Facesmash!</strong> Create your profile to start comparing and messaging with other users.
        </div>
      )}
      {!isNewUser && (
        <div className="alert alert-light mb-4">
          <strong>Editing profile for:</strong> {currentUser.name} ({currentUser.email})
          <br />
          <small className="text-muted">
            Want to create a new profile? <button 
              className="btn btn-link btn-sm p-0" 
              onClick={() => window.location.href = '/'}
            >
              Click here to logout and create a new profile
            </button>
          </small>
        </div>
      )}
      <form onSubmit={onSubmit}>
        <div className="mb-3">
          <label className="form-label">Name</label>
          <input className="form-control" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="mb-3">
          <label className="form-label">Email</label>
          <input type="email" className="form-control" value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div className="mb-3">
          <label className="form-label">Bio</label>
          <textarea className="form-control" rows={3} value={bio} onChange={(e) => setBio(e.target.value)} />
        </div>
        <div className="mb-3">
          <label className="form-label">Gender</label>
          <select className="form-select" value={gender} onChange={(e) => setGender(e.target.value)}>
            <option value="">Select</option>
            <option value="M">Male</option>
            <option value="F">Female</option>
            <option value="Other">Other</option>
          </select>
        </div>
        <div className="mb-3">
          <label className="form-label">Photo</label>
          <input 
            type="file" 
            accept=".jpg,.jpeg,.png,.gif,.webp,.bmp" 
            className="form-control" 
            onChange={(e) => setPhoto(e.target.files?.[0] ?? null)} 
          />
          <div className="form-text">
            Supported formats: JPG, JPEG, PNG, GIF, WebP, BMP
          </div>
        </div>

        {user?.photoUrl && (
          <div className="mb-3">
            <label className="form-label">Current Photo</label>
            <div>
              <img 
                src={getImageUrl(user.photoUrl)} 
                alt="profile" 
                style={{ height: 120, borderRadius: 8 }} 
                onError={(e) => handleImageError(e, 120, 120)}
              />
            </div>
          </div>
        )}

        {saveMessage && (
          <div className="alert alert-success mb-3">
            {saveMessage}
          </div>
        )}

        <div className="d-flex justify-content-end">
          <button className="btn btn-primary" type="submit" disabled={saving}>
            {saving ? (isNewUser ? "Creating..." : "Saving...") : (isNewUser ? "Create Profile" : "Save")}
          </button>
        </div>
      </form>
    </div>
  );
}

export default Profile;
