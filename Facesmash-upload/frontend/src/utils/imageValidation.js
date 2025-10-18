// Image validation utilities

export const ALLOWED_IMAGE_TYPES = [
  'image/jpeg',
  'image/jpg', 
  'image/png',
  'image/gif',
  'image/webp',
  'image/bmp'
];

export const ALLOWED_EXTENSIONS = [
  '.jpg',
  '.jpeg',
  '.png',
  '.gif',
  '.webp',
  '.bmp'
];

export const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

/**
 * Validates an image file
 * @param {File} file - The file to validate
 * @returns {Object} - Validation result with isValid boolean and error message
 */
export const validateImageFile = (file) => {
  if (!file) {
    return { isValid: false, error: 'No file selected' };
  }

  // Check file type
  if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
    return { 
      isValid: false, 
      error: 'Invalid file type. Only JPG, JPEG, PNG, GIF, WebP, and BMP files are allowed.' 
    };
  }

  // Check file size
  if (file.size > MAX_FILE_SIZE) {
    return { 
      isValid: false, 
      error: 'File size must be less than 10MB.' 
    };
  }

  return { isValid: true, error: null };
};

/**
 * Formats file size for display
 * @param {number} bytes - File size in bytes
 * @returns {string} - Formatted file size
 */
export const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

/**
 * Creates a fallback image URL for when images fail to load
 * @param {number} width - Image width
 * @param {number} height - Image height
 * @returns {string} - Base64 encoded SVG placeholder
 */
export const createFallbackImage = (width = 100, height = 100) => {
  const svg = `
    <svg width="${width}" height="${height}" viewBox="0 0 ${width} ${height}" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect width="${width}" height="${height}" fill="#F3F4F6"/>
      <path d="M${width/2} ${height/4}C${width*0.4} ${height/4} ${width*0.25} ${height*0.35} ${width*0.25} ${height/2}C${width*0.25} ${height*0.65} ${width*0.4} ${height*0.75} ${width/2} ${height*0.75}C${width*0.6} ${height*0.75} ${width*0.75} ${height*0.65} ${width*0.75} ${height/2}C${width*0.75} ${height*0.35} ${width*0.6} ${height/4} ${width/2} ${height/4}Z" fill="#9CA3AF"/>
      <path d="M${width*0.25} ${height*0.625}H${width*0.75}V${height*0.875}C${width*0.75} ${height*0.95} ${width*0.6} ${height} ${width/2} ${height}C${width*0.4} ${height} ${width*0.25} ${height*0.95} ${width*0.25} ${height*0.875}V${height*0.625}Z" fill="#9CA3AF"/>
    </svg>
  `;
  return `data:image/svg+xml;base64,${btoa(svg)}`;
};

/**
 * Handles image load errors by setting a fallback image
 * @param {Event} event - The error event
 * @param {number} width - Fallback image width
 * @param {number} height - Fallback image height
 */
export const handleImageError = (event, width = 100, height = 100) => {
  event.target.src = createFallbackImage(width, height);
};

/**
 * Constructs the proper image URL, handling both external and local URLs
 * @param {string} photoUrl - The photo URL from the database
 * @param {string} baseUrl - The base URL for local images (default: http://localhost:5097)
 * @returns {string} - The complete image URL
 */
export const getImageUrl = (photoUrl, baseUrl = 'http://localhost:5097') => {
  if (!photoUrl) return createFallbackImage();
  
  // If it's already a complete URL (starts with http), use it as is
  if (photoUrl.startsWith('http')) {
    return photoUrl;
  }
  
  // Otherwise, prepend the base URL
  return `${baseUrl}${photoUrl}`;
};
