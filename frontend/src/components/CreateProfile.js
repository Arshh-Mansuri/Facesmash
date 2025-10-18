import React, { useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

function CreateProfile() {
  const [formData, setFormData] = useState({
    name: "",
    bio: "",
    photoUrl: ""
  });
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage("");

    try {
      const res = await axios.post("http://localhost:5097/api/profile/create", formData, {
        withCredentials: true // Important for session cookies
      });

      setMessage("Profile created successfully!");
      
      // Update localStorage with new info
      localStorage.setItem("name", res.data.name || "");

      // Redirect to dashboard after a short delay
      setTimeout(() => {
        navigate("/dashboard");
      }, 1500);

    } catch (err) {
      if (err.response?.status === 401) {
        setMessage("Please log in to create your profile.");
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      } else {
        setMessage("Failed to create profile. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleSkip = () => {
    navigate("/dashboard");
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-8">
          <div className="card">
            <div className="card-body">
              <h2 className="card-title text-center mb-4">Complete Your Profile</h2>
              <p className="text-center text-muted mb-4">
                Add some details to help others get to know you better. You can always update this later.
              </p>
              
              <form onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label className="form-label">Display Name</label>
                  <input
                    type="text"
                    className="form-control"
                    name="name"
                    value={formData.name}
                    onChange={handleChange}
                    placeholder="Enter your display name"
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">Bio</label>
                  <textarea
                    className="form-control"
                    name="bio"
                    value={formData.bio}
                    onChange={handleChange}
                    rows="4"
                    placeholder="Tell us about yourself..."
                    maxLength="500"
                  />
                  <div className="form-text">
                    {formData.bio.length}/500 characters
                  </div>
                </div>

                <div className="mb-3">
                  <label className="form-label">Photo URL</label>
                  <input
                    type="url"
                    className="form-control"
                    name="photoUrl"
                    value={formData.photoUrl}
                    onChange={handleChange}
                    placeholder="https://example.com/your-photo.jpg"
                  />
                  <div className="form-text">
                    You can upload a photo later from your profile page
                  </div>
                </div>

                <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                  <button 
                    type="button" 
                    className="btn btn-outline-secondary me-md-2"
                    onClick={handleSkip}
                    disabled={isLoading}
                  >
                    Skip for Now
                  </button>
                  <button 
                    type="submit" 
                    className="btn btn-primary"
                    disabled={isLoading}
                  >
                    {isLoading ? "Creating Profile..." : "Create Profile"}
                  </button>
                </div>
              </form>

              {message && (
                <div className={`alert mt-3 ${message.includes("successfully") ? "alert-success" : "alert-danger"}`}>
                  {message}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default CreateProfile;
