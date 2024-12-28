from PIL import Image
import os

def resize_images(input_folder='resize', output_folder='resize/output', size=(300, 300)):
    # Créer le dossier de sortie s'il n'existe pas
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)

    # Parcourir toutes les images PNG dans le dossier d'entrée
    for filename in os.listdir(input_folder):
        if filename.endswith('.png'):
            # Chemin complet de l'image
            img_path = os.path.join(input_folder, filename)
            
            # Ouvrir et redimensionner l'image
            with Image.open(img_path) as img:
                resized_img = img.resize(size)
                
                # Sauvegarder l'image redimensionnée dans le dossier de sortie
                output_path = os.path.join(output_folder, filename)
                resized_img.save(output_path)
                print(f"Image '{filename}' redimensionnée et enregistrée dans '{output_path}'")


def rename_images(folder='resize'):
    # Parcourir tous les fichiers dans le dossier
    for filename in os.listdir(folder):
        # Vérifier si le fichier est un fichier PNG et se termine par "_back"
        if filename.endswith('_back.png'):
            # Construire le nouveau nom en remplaçant "_back" par "_front"
            new_filename = filename.replace('_back.png', '_front.png')
            
            # Chemin complet de l'ancien et du nouveau fichier
            old_path = os.path.join(folder, filename)
            new_path = os.path.join(folder, new_filename)
            
            # Renommer le fichier
            os.rename(old_path, new_path)
            print(f"Fichier '{filename}' renommé en '{new_filename}'")


# Exemple d'utilisation
rename_images()

# Exemple d'utilisation
resize_images()
