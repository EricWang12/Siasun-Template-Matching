function ScaleAllImages()

   imgFiles = dir('*.bmp');
   
   for imgFileIdx = 1:1:length(imgFiles)
       fprintf('Obra�ujem %d. sliku...\n', imgFileIdx);
       
       imgFileName = imgFiles(imgFileIdx).name;
       ScaleImage(imgFileName);      
   end

end